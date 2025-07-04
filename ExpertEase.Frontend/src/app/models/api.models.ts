﻿import { Timestamp } from 'firebase/firestore';

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

export interface PaginationSearchQueryParams {
  page: number;
  pageSize: number;
  search?: string; // For name, email, phone, address search
}

export interface SpecialistFilterParams {
  categoryIds?: string[];
  minRating?: number;
  experienceRange?: string;
  sortByRating?: 'asc' | 'desc';
}


export interface SpecialistPaginationQueryParams extends PaginationSearchQueryParams {
  filters?: SpecialistFilterParams;
}
export interface StatusUpdateDTO {
  id: string;
  status: StatusEnum;
}

export interface JobStatusUpdateDTO {
  id: string;
  status: JobStatusEnum;
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

export interface MessageAddDTO {
    content: string;
}

export interface MessageDTO {
    id: string;
    senderId: string;
    content: string;
    isRead: boolean;
    createdAt: Date;
}

export interface LoginResponseDTO {
    token: string;
    user: UserDTO;
}

export interface UserUpdateResponseDTO {
  token: string;
  user: UserUpdateDTO;
}

export interface SocialLoginDTO {
  token: string;
  provider: string; // 'google' or 'facebook'
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
    senderId: string
    requestId: string;
    startDate: Date;
    endDate: Date;
    price: number;
    status: StatusEnum;
    payment?: PaymentDetailsDTO
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
    senderId: string;
    requestedStartDate: Date;
    description: string;
    senderPhoneNumber: string;
    senderAddress: string;
    status: StatusEnum;
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
    replyId: string;
    userId: string;
    specialistId: string;
    specialistFullName: string;
    startDate: Date;
    endDate: Date;
    description: string;
    address: string;
    price: number;
    status: JobStatusEnum;
    completedAt: Date;
    cancelledAt: Date;
}

export interface ReplyPaymentDetailsDTO {
  replyId: string;
  startDate: Date;
  endDate: Date;
  description: string;
  address: string;
  price: number;
  clientId: string;
  specialistId: string;
}

export interface ServicePaymentDetailsDTO {
  replyId: string;
  startDate: Date;
  endDate: Date;
  description: string;
  address: string;
  price: number;
}

export interface UserPaymentDetailsDTO {
  userId: string;
  userFullName: string;
  email: string;
  phoneNumber: string;
}
export interface SaveCustomerPaymentMethodDto {
  paymentMethodId: string;
  cardLast4: string;
  cardBrand: string;
  cardholderName: string;
  isDefault: boolean;
}

/**
 * Interface for customer payment method response
 * Maps to CustomerPaymentMethodDto from backend
 */
export interface CustomerPaymentMethodDto {
  id: string;
  userId: string;
  stripeCustomerId: string;
  stripePaymentMethodId: string;
  cardLast4: string;
  cardBrand: string;
  cardholderName: string;
  isDefault: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface PaymentIntentCreateDTO {
  replyId: string;
  serviceAmount: number;        // NEW: Amount that goes to specialist
  protectionFee?: number;       // NEW: Platform protection fee (calculated if not provided)
  totalAmount?: number;         // NEW: Total amount (calculated if not provided)
  currency?: string;            // default: "ron"
  description?: string;
  metadata?: Record<string, string>;
  protectionFeeDetails?: ProtectionFeeDetailsDTO; // NEW: Fee calculation details
}

export interface ProtectionFeeDetailsDTO {
  baseServiceAmount: number;
  feeType: string;              // 'percentage' or 'fixed'
  feePercentage?: number;
  fixedFeeAmount?: number;
  minimumFee: number;
  maximumFee: number;
  calculatedFee: number;
  feeJustification: string;
  calculatedAt: Date;
}

export interface PaymentIntentResponseDTO {
  clientSecret: string;
  paymentIntentId: string;
  stripeAccountId: string;      // NEW: Specialist's Stripe account
  serviceAmount: number;        // NEW: Amount for specialist
  protectionFee: number;        // NEW: Platform protection fee
  totalAmount: number;          // NEW: Total amount charged
  protectionFeeDetails?: ProtectionFeeDetailsDTO; // NEW: Fee breakdown
}

export interface PaymentConfirmationDTO {
  paymentIntentId: string;
  replyId: string;
  serviceAmount: number;        // NEW: Service amount
  protectionFee: number;        // NEW: Protection fee
  totalAmount: number;          // NEW: Total amount
  paymentMethod?: string;
}

export interface PaymentHistoryDTO {
  id: string;
  replyId: string;

  // NEW: Detailed amount breakdown
  serviceAmount: number;
  protectionFee: number;
  totalAmount: number;

  currency: string;
  status: string;
  paidAt?: string;              // ISO Date string or null
  escrowReleasedAt?: string;    // NEW: When money was released to specialist

  serviceDescription: string;
  serviceAddress: string;
  specialistName: string;
  clientName: string;

  // NEW: Financial tracking
  transferredAmount: number;
  refundedAmount: number;
  isEscrowed: boolean;
}

export interface PaymentDetailsDTO {
  id: string;
  replyId: string;

  // NEW: Detailed amount breakdown
  serviceAmount: number;
  protectionFee: number;
  totalAmount: number;

  currency: string;
  status: string;
  paidAt?: string;
  escrowReleasedAt?: string;    // NEW: When money was released to specialist
  createdAt: string;

  stripePaymentIntentId?: string;
  stripeTransferId?: string;    // NEW: Transfer ID when money sent to specialist
  stripeRefundId?: string;      // NEW: Refund ID if payment was refunded

  serviceDescription: string;
  serviceAddress: string;
  serviceStartDate: Date;
  serviceEndDate: Date;
  specialistName: string;
  clientName: string;

  // NEW: Financial details
  transferredAmount: number;
  refundedAmount: number;
  platformRevenue: number;
  isEscrowed: boolean;
  protectionFeeDetails?: ProtectionFeeDetailsDTO;
}

export interface PaymentRefundDTO {
  paymentId: string;
  amount?: number;              // If null, full refund
  reason?: string;
}

export interface PaymentReleaseDTO {
  paymentId: string;
  customAmount?: number;        // If null, release full service amount
  reason?: string;
}

export interface PaymentReportDTO {
  period: string;
  totalServiceRevenue: number;
  totalProtectionFees: number;
  totalPlatformRevenue: number;
  totalTransactions: number;
  completedServices: number;
  refundedServices: number;
  escrowedPayments: number;
  refundRate: number;
  averageServiceValue: number;
  averageProtectionFee: number;
  totalEscrowedAmount: number;
}

export interface PaymentStatusResponseDTO {
  paymentId: string;
  serviceTaskId: string;
  status: string;
  isEscrowed: boolean;          // NEW: Is money in escrow
  canBeReleased: boolean;       // NEW: Can be released to specialist
  canBeRefunded: boolean;       // NEW: Can be refunded to client
  amountBreakdown: PaymentAmountBreakdown; // NEW: Amount breakdown
  protectionFeeDetails?: ProtectionFeeDetailsDTO;
}

export interface PaymentAmountBreakdown {
  serviceAmount: number;        // Amount for specialist
  protectionFee: number;        // Platform protection fee
  totalAmount: number;          // Total charged to client
  transferredAmount: number;    // Amount already transferred to specialist
  refundedAmount: number;       // Amount already refunded to client
  pendingAmount: number;        // Amount still in escrow
  platformRevenue: number;      // Platform's secured revenue
}

// ✅ NEW: Protection fee calculation DTOs
export interface CalculateProtectionFeeRequestDTO {
  serviceAmount: number;
}

export interface CalculateProtectionFeeResponseDTO {
  serviceAmount: number;
  protectionFee: number;
  totalAmount: number;
  feeJustification: string;
  feeConfiguration: ProtectionFeeConfigurationDTO;
}

export interface ProtectionFeeConfigurationDTO {
  feeType: string;              // 'percentage', 'fixed', 'hybrid'
  percentageRate: number;
  fixedAmount: number;
  minimumFee: number;
  maximumFee: number;
  isEnabled: boolean;
  description: string;
  lastUpdated: Date;
}

export interface DetailedProtectionFeeResponseDTO {
  serviceAmount: number;
  protectionFee: number;
  totalAmount: number;
  breakdown: ProtectionFeeBreakdownDTO;
  configuration: ProtectionFeeConfigurationDTO;
  summary: string;
}

export interface ProtectionFeeBreakdownDTO {
  baseServiceAmount: number;
  feeType: string;
  percentageRate: number;
  fixedAmount: number;
  minimumFee: number;
  maximumFee: number;
  calculatedFeeBeforeLimits: number;
  finalFee: number;
  justification: string;
  minimumApplied: boolean;
  maximumApplied: boolean;
  calculatedAt: Date;
}

export interface ProtectionFeeConfig {
  type: 'percentage' | 'fixed';
  percentage: number;
  fixedAmount: number;
  minFee: number;
  maxFee: number;
  enabled: boolean;
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
    profilePictureUrl?: string;
    phoneNumber: string;
    address: string;
    yearsExperience: number;
    description: string;
    createdAt: Date;
    updatedAt: Date;
    rating: number;
    categories?: CategoryDTO[];
}

export interface StripeAccountStatusDTO {
  accountId: string;
  isActive: boolean;
  chargesEnabled: boolean;
  payoutsEnabled: boolean;
  detailsSubmitted: boolean;
  requirementsCurrentlyDue: string[];
  requirementsEventuallyDue: string[];
  disabledReason?: string;
}

export interface BecomeSpecialistDTO {
    userId: string;
    phoneNumber: string;
    address: string;
    yearsExperience: number;
    description: string;
    categories?: string[] | undefined;
    portfolio?: PortfolioPictureAddDTO[] | undefined; // URLs of portfolio images
}

export interface BecomeSpecialistResponseDTO {
  token: string;
  user: UserDTO;
  stripeAccountId: string;
}
export interface SpecialistProfileDTO {
    yearsExperience: number;
    description: string;
    categories: CategoryDTO[];
    portfolio: string[]; // URLs of portfolio images
    stripeAccountId: string; // Optional, if the specialist has a Stripe account
}

export interface SpecialistProfileUpdateDTO {
    userId: string;
    phoneNumber?: string | undefined;
    address?: string | undefined;
    yearsExperience?: number | undefined;
    description?: string | undefined;
    categories?: string[] | undefined;
    portfolio?: PortfolioPictureAddDTO[] | undefined;
}

export interface SpecialistUpdateDTO {
    id: string;
    fullName?: string | undefined;
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
    Completed = "Completed",
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
    rating: number;
    profilePictureUrl?: string;
    contactInfo?: ContactInfoDTO;
    specialist?: SpecialistProfileDTO;
}

export interface UserProfileDTO {
  id: string;
  fullName: string;
  profilePictureUrl?: string;
  rating: number;
  createdAt: string; // Use string for ISO date format from .NET
  updatedAt: string;

  // Specialist-only fields (null or undefined for clients)
  email: string;
  phoneNumber: string;
  address?: string;
  yearsExperience?: number;
  description?: string;
  stripeAccountId?: string;
  portfolio?: string[];
  categories?: string[];
}

export interface UserDetailsDTO {
  fullName: string;
  profilePictureUrl?: string;
  rating: number;
  reviews: ReviewDTO[];

  // Specialist-only fields (optional / nullable)
  email?: string;
  phoneNumber?: string;
  address?: string;
  yearsExperience?: number;
  description?: string;
  portfolio?: string[];
  categories?: string[];
}

export interface FirestoreConversationItemDTO {
  id: string;
  conversationId: string;
  senderId: string;
  type: 'message' | 'request' | 'reply';
  createdAt: Timestamp;
  data: { [key: string]: any };
}

export interface ConversationItemDTO {
  id: string;
  conversationId: string;
  senderId: string;
  type: 'message' | 'request' | 'reply';
  createdAt: Date;
  data: { [key: string]: any };
}


export interface ConversationDTO {
  conversationId: string;
  userId: string;
  userFullName: string;
  userProfilePictureUrl?: string;
  conversationItems: FirestoreConversationItemDTO[];
}

export interface PhotoDTO {
  id: string;
  senderId: string;
  url: string;
  fileName: string;
  contentType: string;
  caption?: string;
  createdAt: Date;
  fileSize?: number;
}

export interface UserConversationDTO {
  conversationId: string;
  userId: string;
  userFullName: string;
  userProfilePictureUrl?: string;
  lastMessage: string;
  lastMessageAt: Date;
  unreadCount: number;
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

export enum PaymentStatusEnum {
  Pending = 'Pending',
  Processing = 'Processing',
  Escrowed = 'Escrowed',        // NEW: Money held in escrow
  Completed = 'Completed',      // OLD: Kept for compatibility
  Released = 'Released',        // NEW: Money released to specialist
  Failed = 'Failed',
  Cancelled = 'Cancelled',
  Refunded = 'Refunded',
  PartiallyRefunded = 'PartiallyRefunded'
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
    address?: string | undefined;
    phoneNumber?: string | undefined;
}

export interface AdminUserUpdateDTO {
  id: string;
  fullName?: string | undefined;
  role?: UserRoleEnum | undefined;
}

export interface ReviewAddDTO {
  receiverUserId: string;
  rating: number;
  content: string;
}

export interface ReviewDTO {
  id: string;
  senderUserFullName: string,
  senderUserProfilePictureUrl?: string;
  rating: number;
  content: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface PortfolioPictureAddDTO
{
  fileStream: File;
  contentType: string;
  fileName: string;
}

export interface MapMarkerInfo {
  position: google.maps.LatLngLiteral;
  title: string;
  specialist: SpecialistDTO;
  info?: {
    name: string;
    description: string;
    rating: number;
    profilePicture?: string;
    distance?: number;
    address: string;
  };
}

export interface AppNotification {
  id: string;
  type: 'success' | 'info' | 'warning' | 'error';
  message: string;
  timestamp: Date;
  duration?: number; // Auto-dismiss after this many milliseconds
}
