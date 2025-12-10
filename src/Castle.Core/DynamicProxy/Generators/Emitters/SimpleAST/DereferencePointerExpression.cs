// Copyright 2004-2025 Castle Project - http://www.castleproject.org/
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

#if FEATURE_BYREFLIKE

namespace Castle.DynamicProxy.Generators.Emitters.SimpleAST
{
	using System;
	using System.Diagnostics;
	using System.Reflection.Emit;

	internal class DereferencePointerExpression : IExpression
	{
		private readonly IExpression pointerExpression;
		private readonly Type pointerType;

		public DereferencePointerExpression(IExpression pointerExpression, Type pointerType)
		{
			Debug.Assert(pointerType.IsPointer);

			this.pointerExpression = pointerExpression;
			this.pointerType = pointerType;
		}

		public void Emit(ILGenerator gen)
		{
			pointerExpression.Emit(gen);
			gen.Emit(OpCodes.Ldobj, pointerType.GetElementType());
		}
	}
}

#endif
