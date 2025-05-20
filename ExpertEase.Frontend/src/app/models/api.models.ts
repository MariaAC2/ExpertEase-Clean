export interface RequestResponse<T> {
  response?: T;
  errorMessage?: ErrorMessage;
}

export interface PagedResponse<T> {
  page: number;
  pageSize: number;
  totalCount: number;
  data?: T[];
}

export interface AccountDTO {
    id: string;
    currency: string;
    balance: number;
}

export interface AccountUpdateDTO {
    userId: string;
    currency?: string | undefined;
    amount?: number | undefined;
}

export interface CategoryAddDTO {
    name: string;
    description?: string | undefined;
}

export interface CategoryAdminDTO {
    id: string;
    name: string;
    description?: string | undefined;
    specialistsCount: number;
    specialistIds: string[];
}

export interface CategoryDTO {
    id: string;
    name: string;
    description?: string | undefined;
}

export interface CategorySpecialistDTO {
    name: string;
}

export interface CategoryUpdateDTO {
    id: string;
    name?: string | undefined;
    description?: string | undefined;
}

export interface ContactInfoDTO {
    phoneNumber: string;
    address: string;
}

export enum ErrorCodes {
    Unknown = "Unknown",
    TechnicalError = "TechnicalError",
    EntityNotFound = "EntityNotFound",
    PhysicalFileNotFound = "PhysicalFileNotFound",
    UserAlreadyExists = "UserAlreadyExists",
    WrongPassword = "WrongPassword",
    WrongUser = "WrongUser",
    CannotAdd = "CannotAdd",
    CannotUpdate = "CannotUpdate",
    CannotDelete = "CannotDelete",
    MailSendFailed = "MailSendFailed",
    AccountAlreadyExists = "AccountAlreadyExists",
    EntityAlreadyExists = "EntityAlreadyExists",
}

export interface ErrorMessage {
    message?: string;
    code?: ErrorCodes;
    status?: HttpStatusCode;
}

export enum HttpStatusCode {
    Continue = "Continue",
    SwitchingProtocols = "SwitchingProtocols",
    Processing = "Processing",
    EarlyHints = "EarlyHints",
    OK = "OK",
    Created = "Created",
    Accepted = "Accepted",
    NonAuthoritativeInformation = "NonAuthoritativeInformation",
    NoContent = "NoContent",
    ResetContent = "ResetContent",
    PartialContent = "PartialContent",
    MultiStatus = "MultiStatus",
    AlreadyReported = "AlreadyReported",
    IMUsed = "IMUsed",
    MultipleChoices = "MultipleChoices",
    MovedPermanently = "MovedPermanently",
    Found = "Found",
    SeeOther = "SeeOther",
    NotModified = "NotModified",
    UseProxy = "UseProxy",
    Unused = "Unused",
    TemporaryRedirect = "TemporaryRedirect",
    PermanentRedirect = "PermanentRedirect",
    BadRequest = "BadRequest",
    Unauthorized = "Unauthorized",
    PaymentRequired = "PaymentRequired",
    Forbidden = "Forbidden",
    NotFound = "NotFound",
    MethodNotAllowed = "MethodNotAllowed",
    NotAcceptable = "NotAcceptable",
    ProxyAuthenticationRequired = "ProxyAuthenticationRequired",
    RequestTimeout = "RequestTimeout",
    Conflict = "Conflict",
    Gone = "Gone",
    LengthRequired = "LengthRequired",
    PreconditionFailed = "PreconditionFailed",
    RequestEntityTooLarge = "RequestEntityTooLarge",
    RequestUriTooLong = "RequestUriTooLong",
    UnsupportedMediaType = "UnsupportedMediaType",
    RequestedRangeNotSatisfiable = "RequestedRangeNotSatisfiable",
    ExpectationFailed = "ExpectationFailed",
    MisdirectedRequest = "MisdirectedRequest",
    UnprocessableEntity = "UnprocessableEntity",
    Locked = "Locked",
    FailedDependency = "FailedDependency",
    UpgradeRequired = "UpgradeRequired",
    PreconditionRequired = "PreconditionRequired",
    TooManyRequests = "TooManyRequests",
    RequestHeaderFieldsTooLarge = "RequestHeaderFieldsTooLarge",
    UnavailableForLegalReasons = "UnavailableForLegalReasons",
    InternalServerError = "InternalServerError",
    NotImplemented = "NotImplemented",
    BadGateway = "BadGateway",
    ServiceUnavailable = "ServiceUnavailable",
    GatewayTimeout = "GatewayTimeout",
    HttpVersionNotSupported = "HttpVersionNotSupported",
    VariantAlsoNegotiates = "VariantAlsoNegotiates",
    InsufficientStorage = "InsufficientStorage",
    LoopDetected = "LoopDetected",
    NotExtended = "NotExtended",
    NetworkAuthenticationRequired = "NetworkAuthenticationRequired",
}

export enum JobStatusEnum {
    Confirmed = "Confirmed",
    Completed = "Completed",
    Cancelled = "Cancelled",
    Reviewed = "Reviewed",
}

export interface LoginDTO {
    email?: string;
    password?: string;
}

export interface LoginResponseDTO {
    token: string;
    user: UserDTO;
}

export enum RejectionReason {
    InvalidSender = "InvalidSender",
    InvalidReceiver = "InvalidReceiver",
    InvalidAmount = "InvalidAmount",
    InsufficientFunds = "InsufficientFunds",
    ExceedsLimit = "ExceedsLimit",
    UserRequest = "UserRequest",
    ManualReview = "ManualReview",
    None = "None",
}

export interface ReplyAddDTO {
    startDate?: Date | undefined;
    endDate: Date;
    price: number;
}

export interface ReplyDTO {
    id: string;
    startDate: Date;
    endDate: Date;
    price: number;
    status: StatusEnum;
}

export interface ReplyUpdateDTO {
    id: string;
    startDate?: Date | undefined;
    endDate?: Date | undefined;
    price?: number | undefined;
}

export interface RequestAddDTO {
    receiverUserId: string;
    requestedStartDate: Date;
    phoneNumber: string;
    address: string;
    description: string;
}

export interface RequestDTO {
    id: string;
    requestedStartDate: Date;
    description: string;
    senderContactInfo?: ContactInfoDTO;
    status: StatusEnum;
    replies?: ReplyDTO[];
}

export interface RequestUpdateDTO {
    id: string;
    requestedStartDate?: Date | undefined;
    phoneNumber?: string | undefined;
    address?: string | undefined;
    description?: string | undefined;
}

export interface ServiceTaskDTO {
    id: string;
    userId: string;
    specialistId: string;
    startDate: Date;
    endDate: Date;
    description: string;
    address: string;
    price: number;
    status: JobStatusEnum;
    completedAt: Date;
    cancelledAt: Date;
}

export interface ServiceTaskUpdateDTO {
    id: string;
    startDate?: Date | undefined;
    endDate?: Date | undefined;
    address?: string | undefined;
    price?: number | undefined;
}

export interface SpecialistAddDTO {
    fullName: string;
    email: string;
    password: string;
    phoneNumber: string;
    address: string;
    yearsExperience: number;
    description: string;
}

export interface SpecialistDTO {
    id: string;
    fullName: string;
    email: string;
    phoneNumber: string;
    address: string;
    yearsExperience: number;
    description: string;
    createdAt: Date;
    updatedAt: Date;
    categories?: CategoryDTO[];
}

export interface SpecialistProfileAddDTO {
    userId: string;
    phoneNumber: string;
    address: string;
    yearsExperience: number;
    description: string;
    categories?: string[] | undefined;
}

export interface SpecialistProfileDTO {
    yearsExperience: number;
    description: string;
    categories: CategoryDTO[];
}

export interface SpecialistProfileUpdateDTO {
    userId: string;
    phoneNumber?: string | undefined;
    address?: string | undefined;
    yearsExperience?: number | undefined;
    description?: string | undefined;
}

export interface SpecialistUpdateDTO {
    id: string;
    firstName?: string | undefined;
    lastName?: string | undefined;
    phoneNumber?: string | undefined;
    address?: string | undefined;
    yearsExperience?: number | undefined;
    description?: string | undefined;
}

export enum StatusEnum {
    Pending = "Pending",
    Accepted = "Accepted",
    Rejected = "Rejected",
    Cancelled = "Cancelled",
    Confirmed = "Confirmed",
    Failed = "Failed",
}

export interface TransactionAddDTO {
    senderUserId?: string | undefined;
    receiverUserId?: string | undefined;
    transactionType: TransactionEnum;
    externalSource?: string | undefined;
    amount: number;
    description?: string | undefined;
}

export interface TransactionDTO {
    id: string;
    initiatorUserId: string;
    initiatorUser: UserTransactionDTO;
    senderUserId?: string | undefined;
    senderUser?: UserTransactionDTO;
    receiverUserId?: string | undefined;
    receiverUser?: UserTransactionDTO;
    transactionType: TransactionEnum;
    externalSource?: string | undefined;
    amount: number;
    description?: string | undefined;
    status: StatusEnum;
    rejectionCode?: RejectionReason;
    rejectionDetails?: string | undefined;
}

export enum TransactionEnum {
    Initial = "Initial",
    Deposit = "Deposit",
    Withdraw = "Withdraw",
    Transfer = "Transfer",
}

export interface TransactionUpdateDTO {
    id: string;
    status?: StatusEnum;
    description?: string | undefined;
}

export interface UserAddDTO {
    fullName: string;
    email: string;
    password: string;
    role: UserRoleEnum;
}

export interface UserDTO {
    id: string;
    fullName: string;
    email: string;
    role: UserRoleEnum;
    roleString?: string;
    createdAt: Date;
    updatedAt: Date;
    contactInfo?: ContactInfoDTO;
    specialist?: SpecialistProfileDTO;
}

export interface UserExchangeDTO {
    id: string;
    fullName: string;
    requests: RequestDTO[];
}

export interface UserRegisterDTO {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
}

export enum UserRoleEnum {
    Admin = "Admin",
    Specialist = "Specialist",
    Client = "Client",
}

export interface UserTransactionDTO {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    role: UserRoleEnum;
}

export interface UserUpdateDTO {
    id: string;
    firstName?: string | undefined;
    lastName?: string | undefined;
    password?: string | undefined;
    specialist?: SpecialistProfileUpdateDTO;
}
