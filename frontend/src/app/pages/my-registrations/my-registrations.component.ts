import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Registration } from '../../models/event.model';
import { EventService } from '../../services/event.service';

@Component({
  selector: 'app-my-registrations',
  standalone: true,
  imports: [DatePipe, RouterLink],
  template: `
    <h1>My registrations</h1>
    @if (registrations().length === 0) {
      <p class="muted">You have not registered for any events yet.</p>
    } @else {
      <table class="card">
        <thead>
          <tr><th>Event</th><th>Starts</th><th>Status</th><th></th></tr>
        </thead>
        <tbody>
          @for (registration of registrations(); track registration.id) {
            <tr>
              <td><a [routerLink]="['/events', registration.eventId]">{{ registration.eventTitle }}</a></td>
              <td>{{ registration.eventStartsAt | date: 'medium' }}</td>
              <td>{{ registration.status }}</td>
              <td>
                @if (registration.status !== 'Cancelled') {
                  <button type="button" class="danger" (click)="cancel(registration.eventId)">Cancel</button>
                }
              </td>
            </tr>
          }
        </tbody>
      </table>
    }
  `
})
export class MyRegistrationsComponent implements OnInit {
  private readonly events = inject(EventService);

  readonly registrations = signal<Registration[]>([]);

  ngOnInit(): void {
    this.load();
  }

  cancel(eventId: number): void {
    this.events.cancel(eventId).subscribe({ next: () => this.load() });
  }

  private load(): void {
    this.events.myRegistrations().subscribe({ next: (list) => this.registrations.set(list) });
  }
}
