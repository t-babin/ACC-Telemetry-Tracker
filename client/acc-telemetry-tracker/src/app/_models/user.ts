export class User {
    id: string;
    username: string;
    serverName: string;
    avatar: string;
    isValid: boolean;
    role: string;
    signupDate: Date;
    fileUploadCount: number;

    constructor(object: any) {
        this.id = object.Id;
        this.username = object.Username;
        this.avatar = object.Avatar;
        this.isValid = object.IsValid;
        this.role = object.Role;
        this.signupDate = object.signupDate;
        this.serverName = object.serverName;
        this.fileUploadCount = object.fileUploadCount;
    }
}
