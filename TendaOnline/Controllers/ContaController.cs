using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TendaOnline.Models.ViewModels;

namespace TendaOnline.Controllers;

public class ContaController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ContaController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
    }
    // GET: /Conta/Login
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST: /Conta/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Senha,
                model.LembrarMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
        }

        return View(model);
    }

    // POST: /Conta/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    // GET: /Conta/AcessoNegado
    [HttpGet]
    public IActionResult AcessoNegado()
    {
        return View();
    }

    // GET: /Conta/Usuarios
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Usuarios()
    {
        var usuarios = await _userManager.Users.ToListAsync<IdentityUser>();
        var listaUsuarios = new List<(IdentityUser Usuario, IList<string> Perfis)>();

        foreach (var user in usuarios)
        {
            var perfis = await _userManager.GetRolesAsync(user);
            listaUsuarios.Add((user, perfis));
        }

        return View(listaUsuarios);
    }

    // GET: /Conta/Registrar
    [Authorize(Roles = "Admin")]
    public IActionResult Registrar()
    {
        return View(new RegistrarUsuarioViewModel());
    }

    // POST: /Conta/Registrar
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistrarUsuarioViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Senha);

            if (result.Succeeded)
            {
                if (await _roleManager.RoleExistsAsync(model.Perfil))
                {
                    await _userManager.AddToRoleAsync(user, model.Perfil);
                }

                TempData["MensagemSucesso"] = $"Usuário '{model.Email}' criado com sucesso como {model.Perfil}!";
                return RedirectToAction(nameof(Usuarios));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    // GET: /Conta/Editar/id
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Editar(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var model = new EditarUsuarioViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Perfil = roles.FirstOrDefault() ?? "Operador"
        };

        return View(model);
    }

    // POST: /Conta/Editar
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarUsuarioViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null) return NotFound();

        user.Email = model.Email;
        user.UserName = model.Email;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        // Atualiza o Perfil/Role
        var perfisAtuais = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, perfisAtuais);
        await _userManager.AddToRoleAsync(user, model.Perfil);

        TempData["MensagemSucesso"] = $"Dados do usuário '{user.Email}' atualizados com sucesso!";
        return RedirectToAction(nameof(Usuarios));
    }

    // POST: /Conta/ResetarSenha
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetarSenha(ResetarSenhaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["MensagemErro"] = "Dados inválidos para redefinição de senha.";
            return RedirectToAction(nameof(Usuarios));
        }

        var user = await _userManager.FindByIdAsync(model.UsuarioId);
        if (user == null) return NotFound();

        // Como é o Administrador redefinindo diretamente, remove a senha antiga e adiciona a nova
        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (removeResult.Succeeded)
        {
            var addResult = await _userManager.AddPasswordAsync(user, model.NovaSenha);
            if (addResult.Succeeded)
            {
                TempData["MensagemSucesso"] = $"Senha do usuário '{user.Email}' redefinida com sucesso!";
                return RedirectToAction(nameof(Usuarios));
            }

            TempData["MensagemErro"] = string.Join("; ", addResult.Errors.Select(e => e.Description));
        }
        else
        {
            TempData["MensagemErro"] = "Falha ao remover credencial anterior.";
        }

        return RedirectToAction(nameof(Usuarios));
    }


    // POST: /Conta/Excluir
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var usuarioLogado = await _userManager.GetUserAsync(User);
        if (usuarioLogado != null && usuarioLogado.Id == id)
        {
            TempData["MensagemErro"] = "Operação negada: Você não pode excluir a sua própria conta logada.";
            return RedirectToAction(nameof(Usuarios));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["MensagemErro"] = "Usuário não encontrado.";
            return RedirectToAction(nameof(Usuarios));
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["MensagemSucesso"] = $"Usuário '{user.Email}' excluído com sucesso!";
        }
        else
        {
            TempData["MensagemErro"] = string.Join("; ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Usuarios));
    }

}