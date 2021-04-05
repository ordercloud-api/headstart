import { UserGroupAssignment } from './UserGroupAssignment';

export interface LocationPermissionUpdate {
    AssignmentsToAdd?: UserGroupAssignment[]
    AssignmentsToDelete?: UserGroupAssignment[]
}