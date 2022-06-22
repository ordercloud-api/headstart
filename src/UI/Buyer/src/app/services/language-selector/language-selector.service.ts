import { Injectable } from '@angular/core'
import { TranslateService } from '@ngx-translate/core'
import { TokenHelperService } from '../token-helper/token-helper.service'
import { CookieService } from 'ngx-cookie'
import { AppConfig } from 'src/app/models/environment.types'
import { CurrentUserService } from '../current-user/current-user.service'

@Injectable({
  providedIn: 'root',
})
export class LanguageSelectorService {
  private languageCookieName = `${this.appConfig.appname
    .replace(/ /g, '_')
    .toLowerCase()}_selectedLang`

  constructor(
    private currentUserService: CurrentUserService,
    private cookieService: CookieService,
    private translate: TranslateService,
    private tokenHelper: TokenHelperService,
    private appConfig: AppConfig
  ) {}

  public async SetLanguage(language: string): Promise<void> {
    if (!this.tokenHelper.isTokenAnonymous()) {
      const user = this.currentUserService.get()
      if (user?.xp?.Language == language) {
        return
      }

      const patchLangXp = {
        xp: {
          Language: language,
        },
      }
      await this.currentUserService.patch(patchLangXp)
    } else {
      const selectedLang = this.cookieService
        .getObject(this.languageCookieName)
        ?.toString()
      if (selectedLang == language) {
        return
      }
    }

    this.cookieService.putObject(this.languageCookieName, language)
    this.SetTranslateLanguage()
  }

  /**
   * Implicitly sets the language used by the translate service
   */
  public async SetTranslateLanguage(): Promise<void> {
    const browserCultureLang = this.translate.getBrowserCultureLang()
    const browserLang = this.translate.getBrowserLang()
    const languages = this.translate.getLangs()
    const selectedLang = this.cookieService
      .getObject(this.languageCookieName)
      ?.toString()
    const isAnonymousUser = this.tokenHelper.isTokenAnonymous()
    let xpLang

    if (!isAnonymousUser) {
      const user = this.currentUserService.get()
      xpLang = user?.xp?.Language
    }
    if (xpLang) {
      this.translate.use(xpLang)
    } else if (selectedLang && languages.includes(selectedLang)) {
      this.translate.use(selectedLang)
    } else if (languages.includes(browserCultureLang)) {
      this.translate.use(browserCultureLang)
    } else if (languages.includes(browserLang)) {
      this.translate.use(browserLang)
    } else if (languages.includes(this.appConfig.defaultLanguage)) {
      this.translate.use(this.appConfig.defaultLanguage)
    } else if (languages.length > 0) {
      this.translate.use(languages[0])
    } else {
      throw new Error('Cannot identify a language to use.')
    }
  }
}
