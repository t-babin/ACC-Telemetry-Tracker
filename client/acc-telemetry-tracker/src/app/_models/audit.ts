export interface AuditLog {
    auditCount: number;
    auditEvents: Audit[];
}

export interface Audit {
    eventDate: Date;
    eventType: string;
    username: string;
    log: string;
}
