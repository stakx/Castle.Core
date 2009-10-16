// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.Applications.MindDump.Presentation.Controllers
{
	using System;

	using Castle.MonoRail.Framework;

	using Castle.Applications.MindDump.Presentation.Filters;
	using Castle.Applications.MindDump.Services;


	[Filter(ExecuteWhen.BeforeAction, typeof(AuthenticationAttemptFilter))]
	[Layout("default")]
	public class IntroController : Controller
	{
		private BlogService _blogService;

		public IntroController(BlogService blogService)
		{
			_blogService = blogService;
		}

		public void Index()
		{
			PropertyBag.Add( "blogs", _blogService.ObtainLatestBlogs() );
			PropertyBag.Add( "posts", _blogService.ObtainLatestPosts() );
		}
	}
}