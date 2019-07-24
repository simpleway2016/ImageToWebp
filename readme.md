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