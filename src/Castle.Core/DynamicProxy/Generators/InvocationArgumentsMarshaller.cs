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
				if (parameters[i].IsByRef && parameters[i].IsReadOnly == false)
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
