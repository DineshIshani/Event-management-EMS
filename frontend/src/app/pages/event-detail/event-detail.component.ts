import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EventItem, Registration } from '../../models/event.model';
import { AuthService } from '../../services/auth.service';
import { EventService } from '../../services/event.service';

@Component({
  selector: 'app-event-detail',
  standalone: true,
  imports: [DatePipe, RouterLink],
  template: `
    @if (event(); as ev) {
      <section class="card">
        <header class="page-header">
          <h1>{{ ev.title }}</h1>
          @if (isOwner(ev)) {
            <span class="actions">
              <a class="button" [routerLink]="['/events', ev.id, 'edit']">Edit</a>
              <button type="button" class="danger" (click)="remove(ev.id)">Delete</button>
            </span>
          }
        </header>
        <p class="muted">
          {{ ev.startsAt | date: 'medium' }} – {{ ev.endsAt | date: 'medium' }} ·
          {{ ev.location || 'Online' }} · organized by {{ ev.organizerName }}
        </p>
        <p>{{ ev.description }}</p>
        <p class="meta">
          <span>{{ ev.registeredCount }}/{{ ev.capacity }} registered</span>
          <span>{{ ev.spotsLeft }} spots left</span>
        </p>

        @if (message()) {
          <p class="success">{{ message() }}</p>
        }
        @if (error()) {
          <p class="error">{{ error() }}</p>
        }

        @if (auth.isLoggedIn()) {
          @if (ev.isRegistered) {
            <button type="button" class="danger" (click)="cancel(ev.id)">Cancel registration</button>
          } @else {
            <button type="button" (click)="register(ev.id)">
              {{ ev.spotsLeft === 0 ? 'Join waitlist' : 'Register' }}
            </button>
          }
        } @else {
          <p class="muted"><a routerLink="/login">Sign in</a> to register for this event.</p>
        }
      </section>

      @if (isOwner(ev)) {
        <section class="card">
          <h2>Attendees</h2>
          @if (attendees().length === 0) {
            <p class="muted">Nobody has registered yet.</p>
          } @else {
            <table>
              <thead>
                <tr><th>Name</th><th>Email</th><th>Status</th><th>Registered</th></tr>
              </thead>
              <tbody>
                @for (registration of attendees(); track registration.id) {
                  <tr>
                    <td>{{ registration.userFullName }}</td>
                    <td>{{ registration.userEmail }}</td>
                    <td>{{ registration.status }}</td>
                    <td>{{ registration.registeredAt | date: 'short' }}</td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </section>
      }
    } @else {
      <p class="muted">Loading event…</p>
    }
  `
})
export class EventDetailComponent implements OnInit {
  private readonly events = inject(EventService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly auth = inject(AuthService);

  readonly event = signal<EventItem | null>(null);
  readonly attendees = signal<Registration[]>([]);
  readonly message = signal('');
  readonly error = signal('');

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.load(id);
  }

  isOwner(ev: EventItem): boolean {
    return this.auth.user()?.id === ev.organizerId;
  }

  register(id: number): void {
    this.error.set('');
    this.events.register(id).subscribe({
      next: (registration) => {
        this.message.set(
          registration.status === 'Waitlisted'
            ? 'The event is full — you have been added to the waitlist.'
            : 'You are registered.'
        );
        this.load(id);
      },
      error: (err) => this.error.set(err?.error?.message ?? 'Unable to register.')
    });
  }

  cancel(id: number): void {
    this.error.set('');
    this.events.cancel(id).subscribe({
      next: () => {
        this.message.set('Registration cancelled.');
        this.load(id);
      },
      error: (err) => this.error.set(err?.error?.message ?? 'Unable to cancel.')
    });
  }

  remove(id: number): void {
    this.events.remove(id).subscribe({
      next: () => void this.router.navigate(['/events']),
      error: (err) => this.error.set(err?.error?.message ?? 'Unable to delete the event.')
    });
  }

  private load(id: number): void {
    this.events.get(id).subscribe({
      next: (ev) => {
        this.event.set(ev);
        if (this.isOwner(ev)) {
          this.events.attendees(ev.id).subscribe({ next: (list) => this.attendees.set(list) });
        }
      },
      error: () => this.error.set('Event not found.')
    });
  }
}
