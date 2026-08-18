export interface AuthUser {
  id: number;
  email: string;
  fullName: string;
  role: 'Organizer' | 'Attendee';
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: AuthUser;
}

export interface LoginPayload {
  email: string;
  password: string;
}

export interface RegisterPayload {
  email: string;
  fullName: string;
  password: string;
  role: 'Organizer' | 'Attendee';
}
