当用户访问.png .jpg文件时，根据客户浏览器对webp格式的支持程度，自动压缩为webp图片，响应给用户，
不支持webp的用户，将以传统图片压缩方式压缩图片，响应给用户

``` 
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ImageToWebp.Factory.Enable(app, env);

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });

            
        }
```