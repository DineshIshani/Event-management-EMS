import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EventService } from '../../services/event.service';

@Component({
  selector: 'app-event-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <section class="card form-card">
      <h1>{{ eventId() ? 'Edit event' : 'New event' }}</h1>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>
          Title
          <input type="text" formControlName="title" />
        </label>
        <label>
          Description
          <textarea rows="4" formControlName="description"></textarea>
        </label>
        <label>
          Location
          <input type="text" formControlName="location" />
        </label>
        <label>
          Starts at
          <input type="datetime-local" formControlName="startsAt" />
        </label>
        <label>
          Ends at
          <input type="datetime-local" formControlName="endsAt" />
        </label>
        <label>
          Capacity
          <input type="number" min="1" formControlName="capacity" />
        </label>
        @if (error()) {
          <p class="error">{{ error() }}</p>
        }
        <button type="submit" [disabled]="form.invalid || saving()">
          {{ saving() ? 'Saving…' : 'Save event' }}
        </button>
      </form>
    </section>
  `
})
export class EventFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly events = inject(EventService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly eventId = signal<number | null>(null);
  readonly saving = signal(false);
  readonly error = signal('');

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    location: [''],
    startsAt: ['', [Validators.required]],
    endsAt: ['', [Validators.required]],
    capacity: [50, [Validators.required, Validators.min(1)]]
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.eventId.set(Number(id));
    this.events.get(Number(id)).subscribe({
      next: (ev) =>
        this.form.patchValue({
          title: ev.title,
          description: ev.description,
          location: ev.location,
          startsAt: toLocalInput(ev.startsAt),
          endsAt: toLocalInput(ev.endsAt),
          capacity: ev.capacity
        }),
      error: () => this.error.set('Event not found.')
    });
  }

  submit(): void {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    const payload = {
      ...value,
      startsAt: new Date(value.startsAt).toISOString(),
      endsAt: new Date(value.endsAt).toISOString()
    };

    this.saving.set(true);
    this.error.set('');
    const id = this.eventId();
    const request$ = id ? this.events.update(id, payload) : this.events.create(payload);

    request$.subscribe({
      next: (ev) => void this.router.navigate(['/events', ev.id]),
      error: (err) => {
        this.error.set(err?.error?.message ?? 'Unable to save the event.');
        this.saving.set(false);
      }
    });
  }
}

function toLocalInput(value: string): string {
  const date = new Date(value);
  const offset = date.getTimezoneOffset() * 60000;
  return new Date(date.getTime() - offset).toISOString().slice(0, 16);
}
