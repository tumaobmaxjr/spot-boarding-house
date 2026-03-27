export type LoginRequest = {
  email: string;
  password: string;
};

export type LoginResponse = {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  roleId: string;
  role: string;
  email: string;
  firstName: string;
  lastName: string;
};
