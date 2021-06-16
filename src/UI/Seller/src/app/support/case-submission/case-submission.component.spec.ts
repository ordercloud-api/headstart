import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'

import { CaseSubmissionComponent } from './case-submission.component'

describe('CaseSubmissionComponent', () => {
  let component: CaseSubmissionComponent
  let fixture: ComponentFixture<CaseSubmissionComponent>

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CaseSubmissionComponent ]
    })
    .compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(CaseSubmissionComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
