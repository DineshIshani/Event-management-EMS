import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { EventItem, EventPayload, Registration } from '../models/event.model';

@Injectable({ providedIn: 'root' })
export class EventService {
  constructor(private readonly http: HttpClient) {}

  list(search = '', upcomingOnly = false): Observable<EventItem[]> {
    let params = new HttpParams();
    if (search) {
      params = params.set('search', search);
    }
    if (upcomingOnly) {
      params = params.set('upcomingOnly', true);
    }

    return this.http.get<EventItem[]>(`${environment.apiUrl}/events`, { params });
  }

  get(id: number): Observable<EventItem> {
    return this.http.get<EventItem>(`${environment.apiUrl}/events/${id}`);
  }

  create(payload: EventPayload): Observable<EventItem> {
    return this.http.post<EventItem>(`${environment.apiUrl}/events`, payload);
  }

  update(id: number, payload: EventPayload): Observable<EventItem> {
    return this.http.put<EventItem>(`${environment.apiUrl}/events/${id}`, payload);
  }

  remove(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/events/${id}`);
  }

  attendees(eventId: number): Observable<Registration[]> {
    return this.http.get<Registration[]>(`${environment.apiUrl}/events/${eventId}/registrations`);
  }

  myRegistrations(): Observable<Registration[]> {
    return this.http.get<Registration[]>(`${environment.apiUrl}/registrations/me`);
  }

  register(eventId: number): Observable<Registration> {
    return this.http.post<Registration>(`${environment.apiUrl}/registrations/events/${eventId}`, {});
  }

  cancel(eventId: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/registrations/events/${eventId}`);
  }
}
