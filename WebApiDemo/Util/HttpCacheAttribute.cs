
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Security.Permissions;
using System.Web;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using CNet.Common;
using WebApiDemo.Model;

namespace WebApiDemo
{
	public class HttpCacheAttribute : Attribute, IResourceFilter
	{
		public int CacheTime { get; set; } //= 10 * 60;

		////方式一:[TypeFilter(typeof(HttpCacheAttribute))]//构造函数注入IMemoryCache，不能使用参数,仅只能有这个构造函数不能有其他。
		//public IMemoryCache _Cache { get; set; }
		//      public HttpCacheAttribute(IMemoryCache cache)
		//{
		//	_Cache = cache;
		//}
		////方式二：[HttpCache]//构造函数注入 创建静态IMemoryCache，可使用参数
		private static IMemoryCache _Cache;
		private static readonly object Locker = new object();
		LogHelper log = LogFactory.GetLoggerSingleton();
		public HttpCacheAttribute() : this(60 * 60)
		{
		}
		public HttpCacheAttribute(int cacheTime)
		{
			this.CacheTime = cacheTime;
			_Cache = Instance;
		}

		///// <summary>
		///// 单例获取
		///// </summary>
		public static IMemoryCache Instance
		{
			get
			{
				if (_Cache == null)
				{
					lock (Locker)
					{
						if (_Cache == null)
						{
							_Cache = new MemoryCache(new MemoryCacheOptions());
						}
					}
				}
				return _Cache;
			}
		}

		public void OnResourceExecuted(ResourceExecutedContext context)
		{
			var key = GetKey(context.HttpContext.Request);

			if (_Cache.Get(key) == null)
			{
				var repResult = context.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
				try
				{
					if (repResult==null)
					{
						log.Error("repResult is NULL");
						return;
					}
					if (repResult.Value==null)
					{
						return;
					}
					var repResultValue =JsonConvert.DeserializeObject<DataRes<dynamic>>(JsonConvert.SerializeObject(repResult.Value));
					if (context.HttpContext.Request.ContentType!=null&& context.HttpContext.Request.ContentType.StartsWith("multipart/form-data"))
					{
                        _Cache.Set(key, repResult, new DateTimeOffset(DateTime.Now.AddSeconds(CacheTime)));
                    }
					else if (repResultValue.code== ResCode.Success&&repResultValue.data!=null)
					{
						_Cache.Set(key, repResult, new DateTimeOffset(DateTime.Now.AddSeconds(CacheTime)));
					}
				}
				catch (Exception ex)
				{
					log.Error($" OnResourceExecuted ERROR: {ex.Message}");
					throw;
				}
			}

			context.Result = _Cache.Get(key) as IActionResult;
		}


		public void OnResourceExecuting(ResourceExecutingContext context)
		{
			var key = GetKey(context.HttpContext.Request);
			if (_Cache.Get(key) != null)
			{
				var repResult = _Cache.Get(key) as IActionResult;
				context.Result = repResult;
				return;
			}
		}
		private string GetKey(HttpRequest request) 
		{
			var path = request.Path;
			var query = request.QueryString;

			string fileName = "";
			if (request.ContentType!=null&& request.ContentType.StartsWith("multipart/form-data"))
			{
                fileName = request?.Form.Files.FirstOrDefault()?.FileName;
                if (!string.IsNullOrWhiteSpace(fileName) && fileName.Length >= 32) { fileName = fileName.Substring(fileName.Length - 32); }
            }
		

            var key = HttpUtility.UrlEncode($"{path}q={query}fn={fileName}");
			return key;
		}
	}
}
