import { Injectable } from '@angular/core'
import { Resolve } from '@angular/router'
import { BaseResolveService } from '../services/base-resolve/base-resolve.service'

@Injectable({
  providedIn: 'root',
})
export class BaseResolve implements Resolve<any> {
  constructor( private baseResolveService: BaseResolveService ) {}

  async resolve(): Promise<void> {
    return this.baseResolveService.resolve()
   }
}
