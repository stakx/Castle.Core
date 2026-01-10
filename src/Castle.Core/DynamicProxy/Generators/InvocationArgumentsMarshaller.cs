// Copyright 2004-2026 Castle Project - http://www.castleproject.org/
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.DynamicProxy.Generators
{
	using System.Linq;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Runtime.InteropServices;

	using Castle.DynamicProxy.Generators.Emitters;
	using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
	using Castle.DynamicProxy.Internal;

	internal class InvocationArgumentsMarshaller
	{
		private readonly MethodEmitter method;
		private readonly ParameterInfo[] parameters;

		public InvocationArgumentsMarshaller(MethodEmitter method, ParameterInfo[] parameters)
		{
			this.method = method;
			this.parameters = parameters;
		}

		public void CreateArgumentsArray(out LocalReference argumentsArray)
		{
			argumentsArray = method.CodeBuilder.DeclareLocal(typeof(object[]));

			method.CodeBuilder.AddStatement(
				new AssignStatement(
					argumentsArray,
					new NewArrayExpression(method.Arguments.Length, typeof(object))));

		}

		public void MarshalArgumentsInto(LocalReference argumentsArray)
		{
			method.CodeBuilder.AddStatement(
				new MarshalArgumentsIntoStatement(method.Arguments, argumentsArray));
		}

		public void MarshalArgumentsOutFrom(LocalReference argumentsArray)
		{
			var arguments = method.Arguments;
			for (int i = 0, n = arguments.Length; i < n; ++i)
			{
				if (IsByRef(parameters[i]) && !IsReadOnly(parameters[i]))
				{
					Reference argument = new IndirectReference(arguments[i]);

#if FEATURE_BYREFLIKE
					if (argument.Type.IsByRefLikeSafe())
					{
						// The argument value in the invocation `Arguments` array is an `object`
						// and cannot be converted back to its original by-ref-like type.
						// We need to replace it with some other value.

						// For now, we just substitute the by-ref-like type's default value:
						if (parameters[i].IsOut)
						{
							method.CodeBuilder.AddStatement(
								new AssignStatement(
									argument,
									new DefaultValueExpression(argument.Type)));
						}
						else
						{
							// ... except when we're dealing with a `ref` parameter. Unlike with `out`,
							// where we would be expected to definitely assign to it, we are free to leave
							// the original incoming value untouched. For now, that's likely the better
							// interim solution than unconditionally resetting.
						}
					}
					else
#endif
					{
						method.CodeBuilder.AddStatement(
							new AssignStatement(
								argument,
								new ConvertExpression(
									argument.Type,
									new ArrayElementReference(argumentsArray, i))));
					}
				}
			}

			bool IsByRef(ParameterInfo parameter)
			{
				return parameter.ParameterType.IsByRef;
			}

			bool IsReadOnly(ParameterInfo parameter)
			{
				// C# `in` parameters are also by-ref, but meant to be read-only.
				// The section "Metadata representation of in parameters" on the following page
				// defines how such parameters are marked:
				//
				// https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.2/readonly-ref.md
				//
				// This poses three problems for detecting them:
				//
				//  * The C# Roslyn compiler marks `in` parameters with an `[in]` IL modifier,
				//    but this isn't specified, nor is it used uniquely for `in` params.
				//
				//  * `System.Runtime.CompilerServices.IsReadOnlyAttribute` is not defined on all
				//    .NET platforms, so the compiler sometimes recreates that type in the same
				//    assembly that contains the method having an `in` parameter. In other words,
				//    it's an attribute one must check for by name (which is slow, as it implies
				//    use of a `GetCustomAttributes` enumeration instead of a faster `IsDefined`).
				//
				//  * A required custom modifier `System.Runtime.InteropServices.InAttribute`
				//    is always present in those cases relevant for DynamicProxy (proxyable methods),
				//    but not all targeted platforms support reading custom modifiers. Also,
				//    support for cmods is generally flaky (at this time of writing, mid-2018).
				//
				// The above points inform the following detection logic: First, we rely on an IL
				// `[in]` modifier being present. This is a "fast guard" against non-`in` parameters:
				if ((parameter.Attributes & (ParameterAttributes.In | ParameterAttributes.Out)) != ParameterAttributes.In)
				{
					return false;
				}

				// This check allows to make the detection logic more robust on the platforms which support custom modifiers.
				// The robustness is achieved by the fact, that usually the `IsReadOnlyAttribute` emitted by the compiler is internal to the assembly.
				// Therefore, if clients use Reflection.Emit to create "a copy" of the methods with read-only members, they cannot re-use the existing attribute.
				// Instead, they are forced to emit their own `IsReadOnlyAttribute` to mark some argument as immutable.
				// The `InAttribute` type OTOH was always available in BCL. Therefore, it's much easier to copy the modreq and be recognized by Castle.
				//
				// If check fails, resort to the IsReadOnlyAttribute check.
				// Check for the required modifiers first, as it's faster.
				if (parameter.GetRequiredCustomModifiers().Any(x => x == typeof(InAttribute)))
				{
					return true;
				}

				// The comparison by name is intentional; any assembly could define that attribute.
				// See explanation in comment above.
				if (parameter.GetCustomAttributes(false).Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.IsReadOnlyAttribute"))
				{
					return true;
				}

				return false;
			}
		}


		private sealed class MarshalArgumentsIntoStatement : IStatement
		{
			private readonly ArgumentReference[] arguments;
			private readonly LocalReference argumentsArray;

			public MarshalArgumentsIntoStatement(ArgumentReference[] arguments, LocalReference argumentsArray)
			{
				this.arguments = arguments;
				this.argumentsArray = argumentsArray;
			}

			public void Emit(ILGenerator gen)
			{
				for (int i = 0, n = arguments.Length; i < n; ++i)
				{
					argumentsArray.Emit(gen);
					gen.Emit(OpCodes.Ldc_I4, i);

					Reference arg = arguments[i];
					if (arg.Type.IsByRef)
					{
						arg = new IndirectReference(arg);
					}

#if FEATURE_BYREFLIKE
					if (arg.Type.IsByRefLikeSafe())
					{
						// The by-ref-like argument value cannot be put into the `object[]` array,
						// because it cannot be boxed. We need to replace it with some other value.

						// For now, we just erase it by substituting `null`:
						gen.Emit(OpCodes.Ldnull);
						gen.Emit(OpCodes.Stelem_Ref);

						continue;
					}
#endif

					arg.Emit(gen);

					if (arg.Type.IsValueType)
					{
						gen.Emit(OpCodes.Box, arg.Type);
					}
					else if (arg.Type.IsGenericParameter)
					{
						gen.Emit(OpCodes.Box, arg.Type);
					}

					gen.Emit(OpCodes.Stelem_Ref);
				}
			}
		}
	}
}
