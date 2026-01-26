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

#if FEATURE_BYREFLIKE

#nullable enable

using System;

namespace Castle.DynamicProxy
{
#if NET9_0_OR_GREATER
	/// <summary>
	///   Permits indirect access to by-ref-like argument values during method interception.
	/// </summary>
	/// <remarks>
	///   Instances of by-ref-like (<c>ref struct</c>) types live exclusively on the evaluation stack.
	///   Therefore, they cannot be boxed and put into the <see langword="object"/>-typed <see cref="IInvocation.Arguments"/> array.
	///   DynamicProxy replaces these unboxable values with <see cref="IByRefLikeProxy{TByRefLike}"/> references
	///   (or alternatively, in the case of spans, with <see cref="ISpanProxy{T}"/> or <see cref="IReadOnlySpanProxy{T}"/>),
	///   which grant you indirect read/write access to the actual values.
	///   <para>
	///     These references are only valid for the duration of the intercepted method call.
	///     Any attempt to use it beyond that will result in a <see cref="AccessViolationException"/>.
	///   </para>
	/// </remarks>
	/// <typeparam name="TByRefLike">A by-ref-like (<c>ref struct</c>) type.</typeparam>
	public interface IByRefLikeProxy<TByRefLike> where TByRefLike : struct, allows ref struct
	{
		ref TByRefLike Value { get; }
	}
#endif

	/// <summary>
	///   Permits indirect access to <see cref="ReadOnlySpan{T}"/>-typed argument values during method interception.
	/// </summary>
	/// <remarks>
	///   <see cref="ReadOnlySpan{T}"/> is a by-ref-like (<c>ref struct</c>) type, which means that
	///   instances of it live exclusively on the evaluation stack. Therefore, they cannot be boxed
	///   and put into the <see langword="object"/>-typed <see cref="IInvocation.Arguments"/> array.
	///   DynamicProxy replaces these unboxable values with <see cref="IReadOnlySpanProxy{T}"/> references,
	///   which grant you indirect read/write access to the actual value.
	///   <para>
	///     These references are only valid for the duration of the intercepted method call.
	///     Any attempt to use it beyond that will result in a <see cref="AccessViolationException"/>.
	///   </para>
	/// </remarks>
	public interface IReadOnlySpanProxy<T>
	{
		ref ReadOnlySpan<T> Value { get; }
	}

	/// <summary>
	///   Permits indirect access to <see cref="Span{T}"/>-typed argument values during method interception.
	/// </summary>
	/// <remarks>
	///   <see cref="Span{T}"/> is a by-ref-like (<c>ref struct</c>) type, which means that
	///   instances of it live exclusively on the evaluation stack. Therefore, they cannot be boxed
	///   and put into the <see langword="object"/>-typed <see cref="IInvocation.Arguments"/> array.
	///   DynamicProxy replaces these unboxable values with <see cref="ISpanProxy{T}"/> references,
	///   which grant you indirect read/write access to the actual value.
	///   <para>
	///     These references are only valid for the duration of the intercepted method call.
	///     Any attempt to use it beyond that will result in a <see cref="AccessViolationException"/>.
	///   </para>
	/// </remarks>
	public interface ISpanProxy<T>
	{
		ref Span<T> Value { get; }
	}
}

#endif
