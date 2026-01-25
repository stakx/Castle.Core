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
#pragma warning disable CS8500

namespace Castle.DynamicProxy.Internal
{
	using System;
	using System.ComponentModel;
	using System.Threading;

	// This file contains a set of `unsafe` types used at runtime by DynamicProxy proxies to represent by-ref-like values
	// in an `IInvocation`. Such values live exclusively on the evaluation stack and therefore cannot be boxed. Thus they are
	// in principle incompatible with `IInvocation` and we need to replace them with something else... namely these types here.
	//
	// What follows are the safety considerations that went into the design of these types.
	//
	// *) These types use unmanaged pointers (`void*`) to reference storage locations (of by-ref-like method parameters).
	//
	// *) Unmanaged pointers are generally unsafe when used to reference unpinned heap-allocated objects.
	//    These types here should NEVER reference heap-allocated objects. We attempt to enforce this by asking for the
	//    `type` of the storage location, and throw for anything other than by-ref-like types (which by definition cannot
	//    live on the heap).
	//
	// *) Unmanaged pointers can be safe when used to reference stack-allocated objects. However, that is only true
	//    when they point into "live" stack frames. That is, they MUST NOT reference parameters or local variables
	//    of methods that have already finished executing. This is why we have the `ByRefLikeProxy.Invalidate` method:
	//    DynamicProxy (or whatever else instantiated a `ByRefLikeProxy` object to point at a method parameter or local
	//    variable) must invoke this method before said method returns (or tail-calls).
	//
	// *) The `checkType` / `checkPtr` arguments of `GetPtr` or `Invalidate`, respectively, have two purposes:
	//
	//     1. DynamicProxy, or whatever else instantiated a `ByRefLikeProxy`, is expected to know at all times what
	//        exactly each instance references. These parameters make it harder for anyone to use the type directly
	//        if they didn't also instantiate it themselves.
	//
	//     2. `checkPtr` of `Invalidate` attempts to prevent re-use of a referenced storage location for another
	//        similarly-typed local variable by the JIT. DynamicProxy typically instantiates `ByRefLikeProxy` instances
	//        at the start of intercepted method bodies, and it invokes `Invalidate` at the very end, meaning that
	//        the address of the local/parameter is taken at each method boundary, meaning that static analysis should
	//        never during the whole method see the local/parameter as "no longer in use". (This may be a little
	//        paranoid, since the CoreCLR JIT probably exempts so-called "address-exposed" locals from reuse anyway.)
	//
	// *) Finally, we only ever access the unmanaged pointer field through `Volatile` or `Interlocked` to better guard
	//    against cases where someone foolishly copied a `ByRefLikeProxy` instance out of the `IInvocation.Arguments`
	//    and uses it from another thread.
	//
	// As far as I can reason, `ByRefLikeProxy` et al. should be safe to use IFF they are never copied out from an
	// `IInvocation`, and IFF DynamicProxy succeeds in destructing them and erasing them from the `IInvocation` right
	// before the intercepted method finishes executing.

	/// <summary>
	///   Do not use! Only DynamicProxy internals may interact with this class type directly.
	/// </summary>
	[CLSCompliant(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public unsafe class ByRefLikeProxy
	{
		private readonly Type type;
		private nint ptr;

		public ByRefLikeProxy(Type type, void* ptr)
		{
			if (type.IsByRefLikeSafe() == false)
			{
				throw new ArgumentOutOfRangeException(nameof(type));
			}

			if (ptr == null)
			{
				throw new ArgumentNullException(nameof(ptr));
			}

			this.type = type;
			this.ptr = (nint)ptr;
		}

		public void* GetPtr(Type checkType)
		{
			if (checkType != type)
			{
				throw new AccessViolationException();
			}

			return GetPtrNocheck();
		}

		internal void* GetPtrNocheck()
		{
			var ptr = (void*)Volatile.Read(ref this.ptr);

			if (ptr == null)
			{
				throw new AccessViolationException();
			}

			return ptr;
		}

		public void Invalidate(void* checkPtr)
		{
			var ptr = (void*)Interlocked.CompareExchange(ref this.ptr, (nint)null, (nint)checkPtr);

			if (ptr == null || checkPtr != ptr)
			{
				throw new AccessViolationException();
			}
		}
	}

#if NET9_0_OR_GREATER
	/// <summary>
	///   Access instances of this type through the public-facing <see cref="IByRefLikeProxy{TByRefLike}"/> interface.
	///   Only DynamicProxy internals may interact with this class type directly.
	/// </summary>
	[CLSCompliant(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public unsafe class ByRefLikeProxy<TByRefLike> : ByRefLikeProxy, IByRefLikeProxy<TByRefLike>
		where TByRefLike : struct, allows ref struct
	{
		public ByRefLikeProxy(Type type, void* ptr)
			: base(type, ptr)
		{
			if (type != typeof(TByRefLike))
			{
				throw new ArgumentOutOfRangeException(nameof(type));
			}
		}

		public TByRefLike Get()
		{
			return *(TByRefLike*)GetPtrNocheck();
		}

		public void Set(in TByRefLike value)
		{
			*(TByRefLike*)GetPtrNocheck() = value;
		}
	}
#endif

#if !NET9_0_OR_GREATER
	/// <summary>
	///   Access instances of this type through the public-facing <see cref="IReadOnlySpanProxy{T}"/> interface.
	///   Only DynamicProxy internals may interact with this class type directly.
	/// </summary>
#else
	/// <summary>
	///   Access instances of this type through either of the public-facing
	///   <see cref="IReadOnlySpanProxy{T}"/> or <see cref="IByRefLikeProxy{TByRefLike}"/> interfaces.
	///   Only DynamicProxy internals may interact with this class type directly.
	/// </summary>
#endif
	[CLSCompliant(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public unsafe class ReadOnlySpanProxy<T> : ByRefLikeProxy, IReadOnlySpanProxy<T>
#if NET9_0_OR_GREATER
	                                                                                , IByRefLikeProxy<ReadOnlySpan<T>>
#endif
	{
		public ReadOnlySpanProxy(Type type, void* ptr)
			: base(type, ptr)
		{
			if (type != typeof(ReadOnlySpan<T>))
			{
				throw new ArgumentOutOfRangeException(nameof(type));
			}
		}

		public ReadOnlySpan<T> Get()
		{
			return *(ReadOnlySpan<T>*)GetPtrNocheck();
		}

		public void Set(in ReadOnlySpan<T> value)
		{
			*(ReadOnlySpan<T>*)GetPtrNocheck() = value;
		}
	}

#if !NET9_0_OR_GREATER
	/// <summary>
	///   Access instances of this type through the public-facing <see cref="ISpanProxy{T}"/> interface.
	///   Only DynamicProxy internals may interact with this class type directly.
	/// </summary>
#else
	/// <summary>
	///   Access instances of this type through either of the public-facing
	///   <see cref="ISpanProxy{T}"/> or <see cref="IByRefLikeProxy{TByRefLike}"/> interfaces.
	///   Only DynamicProxy internals may interact with this class type directly.
	/// </summary>
#endif
	[CLSCompliant(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public unsafe class SpanProxy<T> : ByRefLikeProxy, ISpanProxy<T>
#if NET9_0_OR_GREATER
	                                                                , IByRefLikeProxy<Span<T>>
#endif
	{
		public SpanProxy(Type type, void* ptr)
			: base(type, ptr)
		{
			if (type != typeof(Span<T>))
			{
				throw new ArgumentOutOfRangeException(nameof(type));
			}
		}

		public Span<T> Get()
		{
			return *(Span<T>*)GetPtrNocheck();
		}

		public void Set(in Span<T> value)
		{
			*(Span<T>*)GetPtrNocheck() = value;
		}
	}
}

#pragma warning restore CS8500

#endif
