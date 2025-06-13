import { Injectable } from '@angular/core';
import {from, Observable} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GeocodingService {
  constructor() { }

  async geocodeAddress(address: string): Promise<google.maps.LatLngLiteral | null> {
    if (!window.google || !window.google.maps) {
      throw new Error('Google Maps API not loaded');
    }

    const geocoder = new google.maps.Geocoder();

    return new Promise((resolve, reject) => {
      geocoder.geocode({ address }, (results, status) => {
        if (status === google.maps.GeocoderStatus.OK && results && results[0]) {
          const location = results[0].geometry.location;
          resolve({
            lat: location.lat(),
            lng: location.lng()
          });
        } else {
          resolve(null);
        }
      });
    });
  }

  geocodeAddresses(addresses: string[]): Observable<(google.maps.LatLngLiteral | null)[]> {
    const promises = addresses.map(address => this.geocodeAddress(address));
    return from(Promise.all(promises));
  }
}
