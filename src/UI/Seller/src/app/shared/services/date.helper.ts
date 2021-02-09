import { NgbDateStruct } from '@ng-bootstrap/ng-bootstrap'

export function transformDateMMDDYYYY(date: NgbDateStruct): string {
  return `${date.month}-${date.day}-${date.year}`
}
