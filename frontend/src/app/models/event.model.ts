export interface EventItem {
  id: number;
  title: string;
  description: string;
  location: string;
  startsAt: string;
  endsAt: string;
  capacity: number;
  registeredCount: number;
  spotsLeft: number;
  organizerId: number;
  organizerName: string;
  isRegistered: boolean;
}

export interface EventPayload {
  title: string;
  description: string;
  location: string;
  startsAt: string;
  endsAt: string;
  capacity: number;
}

export interface Registration {
  id: number;
  eventId: number;
  eventTitle: string;
  eventStartsAt: string;
  userId: number;
  userFullName: string;
  userEmail: string;
  status: 'Confirmed' | 'Cancelled' | 'Waitlisted';
  registeredAt: string;
}
