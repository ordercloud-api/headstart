import { Injectable } from '@angular/core'
import { ContentManagementClient, JDocument } from '@ordercloud/cms-sdk'
import { AppConfig } from 'src/app/models/environment.types'

@Injectable({
  providedIn: 'root',
})
export class StaticPageService {
  pages: JDocument[] = []

  constructor(public appConfig: AppConfig) {}

  async initialize(): Promise<void> {
    try {
      const pageList = await ContentManagementClient.Documents.ListDocuments(
        'cms-page-schema',
        'ApiClients',
        this.appConfig.clientID
      )
      this.pages = pageList.Items
    } catch (e) {
      // might not be an error if its just not configured
      this.pages = []
    }
  }
}
