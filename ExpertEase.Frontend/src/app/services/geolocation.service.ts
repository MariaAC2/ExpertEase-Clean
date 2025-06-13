import { Injectable } from '@angular/core';
import {from, Observable, throwError} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GeolocationService {
  constructor() { }
  getCurrentPosition(): Observable<GeolocationPosition> {
    if (!navigator.geolocation) {
      return throwError(() => new Error('Geolocation is not supported by this browser.'));
    }

    return from(
      new Promise<GeolocationPosition>((resolve, reject) => {
        navigator.geolocation.getCurrentPosition(
          (position) => resolve(position),
          (error) => reject(error),
          {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 60000
          }
        );
      })
    );
  }

  calculateDistance(
    lat1: number,
    lng1: number,
    lat2: number,
    lng2: number
  ): number {
    const R = 6371; // Radius of the Earth in kilometers
    const dLat = this.deg2rad(lat2 - lat1);
    const dLng = this.deg2rad(lng2 - lng1);
    const a =
      Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.cos(this.deg2rad(lat1)) *
      Math.cos(this.deg2rad(lat2)) *
      Math.sin(dLng / 2) *
      Math.sin(dLng / 2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    const distance = R * c; // Distance in kilometers
    return distance;
  }

  private deg2rad(deg: number): number {
    return deg * (Math.PI / 180);
  }
}
