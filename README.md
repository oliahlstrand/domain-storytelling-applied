# Requirements
```sh
dotnet --version 
```
Should be 10 or higher. If not install the dotnet 10 SDK.

# Getting the code
```sh
git clone https://github.com/MartinLindblomForefront/domain-storytelling-applied.git
```

# Building
```sh
cd domain-storytelling-applied
dotnet build
```

# Running tests
```sh
cd domain-storytelling-applied
dotnet test
```

# Structure
Domain objects goes in `TE.DomainStorytellingApplied.Domain` \
Test cases goes in `TE.DomainStorytellingApplied.Domain.Tests`

# The booking PoC
A proof of concept (PoC) of a booking system for municipal venues (classrooms, halls, pitches).
There is no UI, API or database. The only way to run it is through the tests.

## Design in short
| Type | What it is |
|---|---|
| `Venue` | Aggregate root. Owns its `Booking`s and protects "no double booking" |
| `Booking` | Entity inside `Venue`. Reserved, then Confirmed, Expired or Cancelled |
| `Payment` | Separate aggregate. Points to a booking by `BookingId` |
| `TimeSlot`, `Money`, ids | Value objects. `BookerType` is an enum |
| `BookingService` | Application layer. No rules, only coordinates Venue, Payment and the payment gateway |
| `IPaymentGateway` | Contract for the external payment service. `FakePaymentGateway` in the tests simulates it |

## Rules (assumptions, not verified requirements)
- No overlapping bookings, whole hours only, only within opening hours, never in the past
- Municipality pays 0 kr and is confirmed directly. Others pay hourly rate times hours
- A reservation holds the slot for 15 minutes, then the slot is free again
- Late payment: confirmed if the slot is still free and not started, otherwise refund
- Cancel only before start. A confirmed booking gives a full refund

Not included: concurrent bookings, failed payments, permissions, storage.
Comments starting with `SYNTAX:` and `FÖRKLARING:` are learning aids.