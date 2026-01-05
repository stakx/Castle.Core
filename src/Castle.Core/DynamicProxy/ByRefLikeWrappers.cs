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

namespace Castle.DynamicProxy
{
	using System;
	using System.ComponentModel;
	using System.Runtime.CompilerServices;
	using System.Threading;

	public unsafe class ByRefLikeBox
	{
		private nint ptr;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ByRefLikeBox(nint ptr)
		{
			this.ptr = ptr;
		}

		private protected void* Pointer
		{
			get
			{
				var ptr = Volatile.Read(ref this.ptr);

				if (ptr == nint.Zero)
				{
					throw new AccessViolationException(message: "...");  // TODO!
				}

				return (void*)ptr;
			}
		}

		public void Dispose()
		{
			Volatile.Write(ref ptr, nint.Zero);
		}
	}

#if FEATURE_ALLOWS_REF_STRUCT

	public unsafe class ByRefLikeBox<T> : ByRefLikeBox where T : struct, allows ref struct
	{
		internal ByRefLikeBox(nint ptr)
			: base(ptr)
		{
		}

		public T Get()
		{
			return Unsafe.AsRef<T>(Pointer);
		}

		public void Set(in T value)
		{
			Unsafe.AsRef<T>(Pointer) = value;
		}
	}

#endif

	public unsafe sealed class ReadOnlySpanBox<T>
#if FEATURE_ALLOWS_REF_STRUCT
		: ByRefLikeBox<ReadOnlySpan<T>>
#else
		: ByRefLikeBox
#endif
	{
		public ReadOnlySpanBox(nint ptr)
			: base(ptr)
		{
		}

#if !FEATURE_ALLOWS_REF_STRUCT
#pragma warning disable CS8500
		public ReadOnlySpan<T> Get()
		{
			return *((ReadOnlySpan<T>*)Pointer);
		}

		public void Set(in ReadOnlySpan<T> value)
		{
			*((ReadOnlySpan<T>*)Pointer) = value;
		}
#pragma warning restore CS8500
#endif
	}

	public unsafe sealed class SpanBox<T>
#if FEATURE_ALLOWS_REF_STRUCT
		: ByRefLikeBox<Span<T>>
#else
		: ByRefLikeBox
#endif
	{
		public SpanBox(nint ptr)
			: base(ptr)
		{
		}

#if !FEATURE_ALLOWS_REF_STRUCT
#pragma warning disable CS8500
		public Span<T> Get()
		{
			return *((Span<T>*)Pointer);
		}

		public void Set(in Span<T> value)
		{
			*((Span<T>*)Pointer) = value;
		}
#pragma warning restore CS8500
#endif
	}
}

#endif