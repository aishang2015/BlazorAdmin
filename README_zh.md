### BlazorAdmin

一个使用 Blazor 和 MudBlazor 编写的简单管理系统模板。

[English](README.md)

---

#### 特性：

- 支持交互式自动渲染模式

- 实现了基于角色的访问控制（RBAC）

- JWT 身份认证

- 基于 MudBlazor 构建

- 主题明暗及颜色切换

- 本地化支持

- 审计/登录日志

- 模块化设计

- Quartz 支持

---

#### 技术栈：

- .NET 10

- Blazor

- MudBlazor

- Entity Framework Core

---

#### 快速开始：

##### 运行项目：

1. 克隆仓库
2. 进入 src/BlazorAdmin 目录
3. 运行 `dotnet restore` 恢复依赖
4. 运行 `dotnet run --project BlazorAdmin.Web` 启动应用
5. 打开浏览器访问 `https://localhost:37219`

##### 登录凭据：

用户名：BlazorAdmin

密码：BlazorAdmin

##### 从模板创建新项目：

1. 进入目录：
   ```bash
   cd src
   ```

2. 安装模板：
   ```bash
   dotnet new install .
   ```

3. 在其他目录创建新项目：
   ```bash
   dotnet new batpl -n 你的项目名称
   ```

4. 在 src 目录卸载模板：
   ```bash
   dotnet new uninstall .
   ```