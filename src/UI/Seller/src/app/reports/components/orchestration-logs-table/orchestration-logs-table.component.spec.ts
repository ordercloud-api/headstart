import { OrchestrationLogsService } from '@app-seller/reports/orchestration-logs.service'
import { async, ComponentFixture, TestBed } from '@angular/core/testing'

import { OrchestrationLogsTableComponent } from './orchestration-logs-table.component'
import { Router, ActivatedRoute } from '@angular/router'
import { UserContext } from '@app-seller/config/user-context'
import { of } from 'rxjs'

describe('OrchestrationLogsTableComponent', () => {
  let component: OrchestrationLogsTableComponent
  let fixture: ComponentFixture<OrchestrationLogsTableComponent>
  const userContext: UserContext = {
    Me: {},
    UserRoles: ['SupplierReader'],
    UserType: 'type',
  }
  const orchestrationLogService = {
    isSupplierUser() {
      return false
    },
    resourceSubject: of({}),
    getParentResourceID() {
      return 1
    },
    getParentOrSecondaryIDParamName() {},
  }
  const currentUserService = {
    getUserContext() {
      return userContext
    },
  }
  const router = {
    navigateByUrl: jasmine.createSpy('navigateByUrl'),
    url: { startsWith() {}, split() {} },
    routerState: { snapshot: { url: 'url' } },
  }
  const activatedRoute = { queryParams: of({}), params: of({}) }

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [OrchestrationLogsTableComponent],
      providers: [
        {
          provide: OrchestrationLogsService,
          useValue: orchestrationLogService,
        },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: activatedRoute },
      ],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(OrchestrationLogsTableComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
