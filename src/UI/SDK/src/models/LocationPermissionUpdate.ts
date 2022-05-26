import { UserGroupAssignment } from 'ordercloud-javascript-sdk';

export interface LocationPermissionUpdate {
    AssignmentsToAdd?: UserGroupAssignment[]
    AssignmentsToDelete?: UserGroupAssignment[]
}