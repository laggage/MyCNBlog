## Description

asp.net core3.1博客应用 - 后端api;

支持多用户, 内置三种角色 `ROOT`, `BLOGGER`, `VISITOR`;

ef core + 仓储做数据持久化; 直接使用jwt做身份验证和授权, `AuthController` 官方JWT帮助类签发JWT TOKEN;

## Refactor roadmap

 - [ ] Impl **DDD** architecture
 - [ ] Add integration test
 - [ ] Change to DBFirst
 - [ ] Proxy font end client with asp.net core
 - [ ] Add cache

## Config

部署前需要根据服务器配置设置一下appsettings.json文件,
如下:
```json
{
  "Logging": {  // 内置日志配置
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "urls": "http://*:7757",  // kestrel监听地址

  "Serilog": { // Serilog日志配置
    "Using": [ "Serilog.Sinks.Console" ],
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "Console" }
    ],
    "Enrich": [ "FromLogContext" ],
    "Properties": {
    }
  },
  "Auth": { 
    "PasswordOptions": { // 用户密码强度配置
      "RequiredLength": 6,
      "RequiredUniqueChars": 0,
      "RequireNonAlphanumeric": false,
      "RequireLowercase": false,
      "RequireUppercase": false,
      "RequireDigit": true
    },
    "Security": { // pfx格式证书, 用来加密的
      "CertificatePath": "********",
      "Password": "********"
    },
    "Jwt": {
      "Issuer": "https://www.domain.top:<port>",
      "Audience": "ng-client"
    }
  },
  "Database": { // 数据库连接字符串, 连接字符串放在一个单独文件里, 这里配置一下文件名即可
    "ConnectionStringFilesPath": {
      "mysql": "****.txt"
    }
  },
  "StaticFile": { // 今天文件配置
    "ImageMaxSize": 10, // unit is MB
    "StaticFileRootPath": "dir", // 静态文件根路径
    "StaticFileRequestPath": "/files", // 静态访问根路径
    "DefaultUserAvatarFilePath": "defaultAvatar.svg" // 用户默认头像路径
  },
  "AppCors": { // 跨域配置
    "Policy": "Ng-Client",
    "Origins": [ "*" ], // 通配符仅限测试时使用
    "Headers": [ "*" ],
    "ExposedHeaders": [
      "*"
    ],
    "AllowAnyHeader": true,
    "AllowAnyOrigin": true,
    "Methods": [ "*" ]
  }
}
```

## Deploy

服务器: 阿里云轻量应用服务器 - centos7, 内存只有521M~

服务器安装 dotnet runtime 3.1.x;

建立service文件
`vim /etc/systemd/system/kestrel-MyCNBlog.service`
内容如下:

```conf
[Unit]
Description=MyCNBlog.Api

[Service]
WorkingDirectory=/root/Website/MyCNBlog/publish
ExecStart=/usr/bin/dotnet /root/Website/MyCNBlog/publish/MyCNBlog.Api.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=MyCNBlog
User=root
Environment=ASPNETCORE_ENVIRONMENT=PreProduction
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

service配置里, 并没有将 `ASPNETCORE_ENVIRONMENT` 配置为 `Production`, 因为`Production`模式下, 需要前端配合加密用户密码发送给服务端, 加密这部分api已经完成的, 但是前端暂时还没搞; 后面再说;

启动应用: `systemctl start kestrel-MyCNBlog` ;
查看运行状态: `systemctl status kestrel-MyCNBlog`

配置nginx: 
`vim /etc/nginx/conf.d/cnblog.conf`;

配置如下

```nginx
server {
        listen                    {your port} ssl;
        server_name               www.domain.top;
        ssl_certificate           ****.crt;
        ssl_certificate_key       ****.key;
        ssl_protocols             TLSv1 TLSv1.1 TLSv1.2;
        ssl_prefer_server_ciphers on;
        ssl_ciphers "EECDH+ECDSA+AESGCM EECDH+aRSA+AESGCM EECDH+ECDSA+SHA384 EECDH+ECDSA+SHA256 EECDH+aRSA+SHA384 EECDH+aRSA+SHA256 EECDH+aRSA+RC4 EECDH EDH+aRSA !aNULL !eNULL !LOW !3DES !MD5 !EXP !PSK !SRP !DSS !RC4";

        # ssl_ecdh_curve            secp384r1;
        ssl_session_cache         shared:SSL:10m;
        ssl_session_tickets       off;
        # ssl_stapling              on; #ensure your cert is capable
        # ssl_stapling_verify       on; #ensure your cert is capable

        add_header Strict-Transport-Security "max-age=63072000; includeSubdomains; preload";
        add_header X-Frame-Options DENY;
        add_header X-Content-Type-Options nosniff;

        #Redirects all traffic
        location / {
            proxy_pass http://localhost:7757;
         #   limit_req  zone=one burst=10 nodelay;
        }
    }
```

浏览器访问: `https://www.domain.top:<port>` , 这时可以看到swagger文档页面;

之后测试过程中, 经常碰到500, 或者根本连不上服务器; ssh连接上去排查了一下, 发现mysql和nginx交替发生"宕机", 查看一下mysql的日志, 如下:
![image](https://user-images.githubusercontent.com/38829279/86241070-ddfc9400-bbd4-11ea-8f6b-61a5876ddbb5.png)

**可以看到, 第一行提示, 缓存池内存分配失败...**
难怪了, 服务器只有512M的内存, 上面却跑了5个(加上这个一共五个)asp.net core应用, .net对内存占用似乎一直都不太友好, 再加上, 还有两个代理应用; 没办法, 只好先停掉两个net core应用; 其中四个有net core应用是用supervisor管理的, 感觉supervisor本身可能也比较吃内存, python嘛. 停掉supervisor管理的应用中的两个, 然后重启mysql, nginx(在这之前也尝试重启了十来次了), 继续测试... 好像正常了;

## 前端

前端为了快速完成功能, 有的地方的实现不太优雅, 后面有空再优化把, 先把功能实现了, 渐进式开发嘛. [前端仓库;](https://github.com/laggage/my-cnblog-ng)
