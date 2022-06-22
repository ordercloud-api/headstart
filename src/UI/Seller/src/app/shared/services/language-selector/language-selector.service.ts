import { Inject, Injectable } from '@angular/core'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { TranslateService } from '@ngx-translate/core'
import { CookieService } from 'ngx-cookie'
import { Me } from 'ordercloud-javascript-sdk'
import { AppConfig } from '@app-seller/models/environment.types'
import { OcTokenService } from '@ordercloud/angular-sdk'
import { HSMeUser } from '@ordercloud/headstart-sdk'

@Injectable({
  providedIn: 'root',
})
export class LanguageSelectorService {
  private languageCookieName = `${this.appConfig.appname
    .replace(/ /g, '_')
    .toLowerCase()}_selectedLang`

  constructor(
    private ocTokenService: OcTokenService,
    private cookieService: CookieService,
    private translate: TranslateService,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {}

  public async SetLanguage(language: string, user?: HSMeUser): Promise<void> {
    if (user?.xp?.Language == language) {
      this.cookieService.put(this.languageCookieName, language)
      return
    }

    const accessToken = this.ocTokenService.GetAccess()
    const patchLangXp = {
      xp: {
        Language: language,
      },
    }
    await Me.Patch(patchLangXp, { accessToken: accessToken })

    this.SetTranslateLanguage()
  }

  /**
   * Implicitly sets the language used by the translate service
   */
  public async SetTranslateLanguage(): Promise<void> {
    const browserCultureLang = this.translate.getBrowserCultureLang()
    const browserLang = this.translate.getBrowserLang()
    const languages = this.translate.getLangs()
    const selectedLang = this.cookieService.get(this.languageCookieName)
    const accessToken = this.ocTokenService.GetAccess()
    let xpLang

    if (accessToken) {
      const user = await Me.Get<HSMeUser>({ accessToken: accessToken })
      xpLang = user?.xp?.Language
    }
    if (xpLang) {
      this.useLanguage(xpLang)
    } else if (selectedLang && languages.includes(selectedLang)) {
      this.useLanguage(selectedLang)
    } else if (languages.includes(browserCultureLang)) {
      this.useLanguage(browserCultureLang)
    } else if (languages.includes(browserLang)) {
      this.useLanguage(browserLang)
    } else if (languages.includes(this.appConfig.defaultLanguage)) {
      this.useLanguage(this.appConfig.defaultLanguage)
    } else if (languages.length > 0) {
      this.useLanguage(languages[0])
    } else {
      throw new Error('Cannot identify a language to use.')
    }
  }

  private useLanguage(language: string) {
    this.translate.use(language)
    this.cookieService.put(this.languageCookieName, language)
  }
}
