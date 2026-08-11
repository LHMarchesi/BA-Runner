using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class LanguageManager : MonoBehaviour
{
    private const string LANGUAGE_KEY = "SelectedLanguage";

    [SerializeField] private string defaultLanguage = "es";

    private IEnumerator Start()
    {
        /*
         * Esperamos a que Unity Localization haya cargado
         * todos los Locales disponibles.
         */
        yield return LocalizationSettings.InitializationOperation;

        string savedLanguage =
            PlayerPrefs.GetString(
                LANGUAGE_KEY,
                defaultLanguage
            );

        ApplyLanguage(
            savedLanguage,
            false
        );
    }

    // =========================================================
    // MÉTODO PARA LOS BOTONES
    // =========================================================

    public void SetLanguage(string localeCode)
    {
        ApplyLanguage(
            localeCode,
            true
        );
    }

    // =========================================================
    // CAMBIO REAL DE IDIOMA
    // =========================================================

    private void ApplyLanguage(
        string localeCode,
        bool save
    )
    {
        if (string.IsNullOrWhiteSpace(localeCode))
            return;

        Locale locale =
            LocalizationSettings
                .AvailableLocales
                .GetLocale(localeCode);

        if (locale == null)
        {
            Debug.LogWarning(
                $"[LanguageManager] No existe el Locale " +
                $"'{localeCode}'."
            );

            return;
        }

        LocalizationSettings.SelectedLocale =
            locale;

        if (save)
        {
            PlayerPrefs.SetString(
                LANGUAGE_KEY,
                localeCode
            );

            PlayerPrefs.Save();
        }

        Debug.Log(
            $"[LanguageManager] Idioma cambiado a: " +
            $"{locale.LocaleName} ({localeCode})"
        );
    }
}