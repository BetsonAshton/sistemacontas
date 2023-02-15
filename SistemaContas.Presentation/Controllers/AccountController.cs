using Bogus;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SistemaContas.Data.Entities;
using SistemaContas.Data.Helpers;
using SistemaContas.Data.Repositories;
using SistemaContas.Messages.Services;
using SistemaContas.Presentation.Models;
using System.Security.Claims;

namespace SistemaContas.Presentation.Controllers
{
    public class AccountController : Controller
    {
        //Account/Login
        public IActionResult Login()
        {
          

            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            //verificar se todos os campos do formulário
            //passasaram nas regras de validação
            if (ModelState.IsValid)
            {
                try
                {
                    //consultar o usuário no banco de dados através do email e da senha
                    var usuarioRepository = new UsuarioRepository();
                    var usuario = usuarioRepository.GetByEmailAndSenha(model.Email, MD5Helper.Encrypt(model.Senha));

                    if(usuario != null) // usuário encontrado
                    {
                        #region Realizar a autenticação do usuário

                        var identityViewModel = new IdentityViewModel();
                        identityViewModel.Id = usuario.Id;
                        identityViewModel.Nome = usuario.Nome;
                        identityViewModel.Email = usuario.Email;
                        identityViewModel.DataHoraAcesso = DateTime.Now;

                        //gravando cookie de autenticação
                        var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, JsonConvert.SerializeObject(identityViewModel))},CookieAuthenticationDefaults.AuthenticationScheme);

                        //gravando o cookie de autenticação
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                        //redirecionando o usuário para / Home/ Index

                        return RedirectToAction("Index", "Home");   
                        #endregion

                    }
                    else // usuário não foi encontrado!
                    {
                        TempData["MensagemAlerta"] = "Acesso negado, usuário não encontrado.";

                    }
                }
                catch (Exception e)
                {
                    TempData["MensagemErro"] = $"Falha ao autenticar usuário: {e.Message}";

                }
            }
            else
            {
                TempData["MensagemAlerta"] = "Ocorreram erros de validação no preenchimento do formulário.";
            }
            return View();
        }

        //Account/Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]//recebe o SUBMIT do formulário
        public IActionResult Register(RegisterViewModel model)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var usuarioRepository = new UsuarioRepository();
                    if (usuarioRepository.GetByEmail(model.Email)!= null)
                    {
                        TempData["MensagemAlerta"] = "O email informado já está cadastrado no sistema, tente outro.";
                    }
                    else { 
                    var usuario = new Usuario();

                    usuario.Id = Guid.NewGuid();
                    usuario.Nome = model.Nome;
                    usuario.Email = model.Email;
                    usuario.Senha = MD5Helper.Encrypt(model.Senha);

                    
                    usuarioRepository.Add(usuario);

                    TempData["MensagemSucesso"] = "Parabéns, sua conta foi cadastrada com suceeso.";
                        ModelState.Clear();// limpar todos os campos do formulário
                    }
                }
                catch(Exception e)
                {
                    TempData["MensagemErro"] = $"Falha ao cadastrar usuário: {e.Message}";
                }

            }
            else
            {
                TempData["MensagemAlerta"] = "Ocorrem erros de validação no preenchimento do formulário.";

            }

            return View();
        }


        //Account/ PasswordRecover
        public IActionResult PasswordRecover()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PasswordRecover(PasswordRecoverViewModel model)
        {
            if (ModelState.IsValid) 
            {
                try
                {
                    //buscar o usuário no banco de dados através do email
                    var usuarioRepository = new UsuarioRepository();
                    var usuario = usuarioRepository.GetByEmail(model.Email);

                    //verificando se o usuário foi encontrado

                    if(usuario !=null)
                    {
                        //gerar um nova senha para o usuário
                        var novaSenha = new Faker().Internet.Password();

                        //enviando a nova senha para o usuário
                        var emailDest = usuario.Email;
                        var assunto = "Recuperação de senha - Sistema Contas";
                        var mensagem = $@"
                            <h3>Olá, {usuario.Nome}</h3>
                            <p>Uma nova senha foi gerada com sucesso para o seu usuário.</p>
                            <p>Acesse o sistema com a senha: {novaSenha}</p>
                            <p>Após acessar o sistema, você pode alterar esta senha para uma nova de sua preferência.</p>
                            <br/>
                            <p>Att, <br/>Equipe Sistema Contas</p>
                        ";

                        //Enviando o email para  o usuário
                        EmailService.EnviarMensagem(emailDest, assunto, mensagem);

                        //atualizando a senha do usuário no banco de dados
                        usuarioRepository.Update(usuario.Id, MD5Helper.Encrypt(novaSenha));

                        TempData["MensagemSucesso"] = "Recuperção de senha realizada com sucesso.";
                        ModelState.Clear();
                    }
                    else
                    {
                        TempData["MensagemAlerta"] = "Usuário não encontrado, verifique o email informado.";
                    }

                }
                catch(Exception e)
                {
                    TempData["MensagemErro"] = "Falha ao recuperar senha: " + e.Message;
                }
            
            }
            return View();
        }

        //Account/Logout
        public IActionResult Logout()
        {
            //destruir o cookie de autenticação(identificação do usuário)
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //redirecionar de volta para a página de login
            return RedirectToAction("Login", "Account");
        }

    }
}
