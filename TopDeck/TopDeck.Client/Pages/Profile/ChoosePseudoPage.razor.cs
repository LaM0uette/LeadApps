using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Helpers.Auth0;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Requesters.AuthUser;
using TopDeck.Contracts.DTO;
using TopDeck.Shared.Components;
using TopDeck.Shared.Services;

namespace TopDeck.Client.Pages.Profile;

public class ChoosePseudoPagePresenter : PresenterBase
{
    #region State
    protected bool IsLoading { get; set; } = true;
    protected bool IsSaving { get; set; }
    protected bool IsAuthenticated { get; set; }

    [Required]
    [MinLength(3, ErrorMessage = "Le pseudo doit contenir au moins 3 caractères.")]
    [MaxLength(24, ErrorMessage = "Le pseudo ne peut pas dépasser 24 caractères.")]
    public string NewUserName { get; set; } = string.Empty;

    protected string? ErrorMessage { get; set; }

    private int _userId;
    private string _userUuid = string.Empty;
    private string _provider = string.Empty;
    private string _oAuthId = string.Empty;
    #endregion

    #region Services
    [Inject] private IAuthUserRequester _authUserRequester { get; set; } = null!;
    [Inject] private IUserService _userService { get; set; } = null!;
    [Inject] private NavigationManager _nav { get; set; } = null!;
    [Inject] private IJSRuntime _js { get; set; } = null!;

    [CascadingParameter]
    private Task<AuthenticationState>? _authenticationStateTask { get; set; }
    #endregion

    protected override async Task OnInitializedAsync()
    {
        try
        {
            if (_authenticationStateTask is null)
            {
                IsLoading = false;
                return;
            }

            AuthenticationState state = await _authenticationStateTask;
            ClaimsPrincipal principal = state.User;
            IsAuthenticated = principal.Identity?.IsAuthenticated ?? false;

            if (!IsAuthenticated)
            {
                IsLoading = false;
                return;
            }

            // Parse provider and oauth id from SUB claim
            string? sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Auth0SubHelper.TryParse(sub, out _provider, out _oAuthId))
            {
                ErrorMessage = "Impossible de récupérer l'identité OAuth.";
                IsLoading = false;
                return;
            }

            // Load current user (by OAuth)
            var user = await _authUserRequester.GetAuthenticatedUserAsync(principal);
            if (user is null)
            {
                ErrorMessage = "Utilisateur introuvable.";
                IsLoading = false;
                return;
            }

            _userId = user.Id;
            _userUuid = user.Uuid;
            const string PlaceholderUserName = "__unknown__";
            NewUserName = string.Equals(user.UserName, PlaceholderUserName, System.StringComparison.Ordinal)
                ? string.Empty
                : user.UserName;
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task UpdatePseudoAsync()
    {
        if (IsSaving) return;
        ErrorMessage = null;

        // minimal validation (in addition to DataAnnotations)
        string candidate = (NewUserName ?? string.Empty).Trim();
        if (candidate.Length < 3)
        {
            ErrorMessage = "Le pseudo doit contenir au moins 3 caractères.";
            return;
        }

        try
        {
            IsSaving = true;

            UserInputDTO dto = new(_provider, _oAuthId, candidate);
            var updated = await _userService.UpdateAsync(_userId, dto);
            if (updated is null)
            {
                ErrorMessage = "Échec de la mise à jour du pseudo.";
                return;
            }

            // persist local flag to stop future redirects
            try
            {
                string key = $"td_pseudo_set_{_userUuid}";
                await _js.InvokeVoidAsync("localStorage.setItem", key, "1");
            }
            catch { /* ignore */ }

            _nav.NavigateTo("/", true);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    protected void Cancel()
    {
        _nav.NavigateTo("/", true);
    }
}
