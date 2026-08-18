import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { EventItem } from '../../models/event.model';
import { AuthService } from '../../services/auth.service';
import { EventService } from '../../services/event.service';

@Component({
  selector: 'app-event-list',
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink],
  template: `
    <header class="page-header">
      <h1>Events</h1>
      @if (auth.isOrganizer()) {
        <a class="button" routerLink="/events/new">New event</a>
      }
    </header>

    <div class="filters">
      <input
        type="search"
        placeholder="Search by title or location"
        [(ngModel)]="search"
        (keyup.enter)="load()"
      />
      <label class="checkbox">
        <input type="checkbox" [(ngModel)]="upcomingOnly" (change)="load()" />
        Upcoming only
      </label>
      <button type="button" (click)="load()">Search</button>
    </div>

    @if (loading()) {
      <p class="muted">Loading events…</p>
    } @else if (events().length === 0) {
      <p class="muted">No events found.</p>
    } @else {
      <ul class="event-grid">
        @for (event of events(); track event.id) {
          <li class="card">
            <h2><a [routerLink]="['/events', event.id]">{{ event.title }}</a></h2>
            <p class="muted">{{ event.startsAt | date: 'medium' }} · {{ event.location || 'Online' }}</p>
            <p>{{ event.description }}</p>
            <p class="meta">
              <span>{{ event.registeredCount }}/{{ event.capacity }} registered</span>
              @if (event.isRegistered) {
                <span class="badge">You're going</span>
              } @else if (event.spotsLeft === 0) {
                <span class="badge badge-warn">Full</span>
              }
            </p>
          </li>
        }
      </ul>
    }
  `
})
export class EventListComponent implements OnInit {
  private readonly events$ = inject(EventService);
  readonly auth = inject(AuthService);

  readonly events = signal<EventItem[]>([]);
  readonly loading = signal(false);

  search = '';
  upcomingOnly = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.events$.list(this.search, this.upcomingOnly).subscribe({
      next: (events) => {
        this.events.set(events);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
