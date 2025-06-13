import { Injectable } from '@angular/core';
import {SpecialistDTO, UserDTO, UserRoleEnum} from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class MapService {
  constructor() { }

  // getMapConfig(): MapConfigDto | null {
  //   const currentUser = this.userContextService.getCurrentUser();
  //
  //   if (!currentUser) {
  //     return null;
  //   }
  //
  //   return {
  //     showSpecialists: currentUser.role === UserRoleEnum.Client,
  //     targetUserType: currentUser.role === UserRoleEnum.Client ? 'specialists' : 'clients',
  //     currentUser
  //   };
  // }
  //
  // async getUserMarkers(userLocation?: google.maps.LatLngLiteral): Promise<MapMarkerDto[]> {
  //   const config = this.getMapConfig();
  //   if (!config) {
  //     throw new Error('User not authenticated');
  //   }
  //
  //   try {
  //     let targetUsers: (UserDTO | SpecialistDTO)[] = [];
  //
  //     if (config.showSpecialists) {
  //       // Current user is a client, show specialists
  //       const specialistsResponse = await this.getSpecialists(1, 100).toPromise();
  //       targetUsers = specialistsResponse?.data || [];
  //     } else {
  //       // Current user is a specialist, show clients
  //       const usersResponse = await this.getAllUsers(1, 100).toPromise();
  //       const allUsers = usersResponse?.data || [];
  //       targetUsers = allUsers.filter(user => user.role === UserRoleEnum.Client);
  //     }
  //
  //     const markers: MapMarkerDto[] = [];
  //
  //     for (const user of targetUsers) {
  //       const address = this.getUserAddress(user);
  //       if (address) {
  //         try {
  //           const position = await this.geocodingService.geocodeAddress(address);
  //           if (position) {
  //             const marker: MapMarkerDto = {
  //               position,
  //               title: user.fullName,
  //               user,
  //               info: {
  //                 name: user.fullName,
  //                 description: this.getUserDescription(user),
  //                 rating: user.rating || 0,
  //                 categories: this.getUserCategories(user),
  //                 profilePicture: user.profilePictureUrl,
  //                 phoneNumber: this.getUserPhone(user),
  //                 address: address,
  //                 email: user.email
  //               }
  //             };
  //
  //             // Add specialist-specific info
  //             if ('yearsExperience' in user) {
  //               marker.info!.yearsExperience = user.yearsExperience;
  //             }
  //
  //             // Calculate distance if user location is provided
  //             if (userLocation) {
  //               const distance = this.geolocationService.calculateDistance(
  //                 userLocation.lat,
  //                 userLocation.lng,
  //                 position.lat,
  //                 position.lng
  //               );
  //               marker.info!.distance = Math.round(distance * 100) / 100;
  //             }
  //
  //             markers.push(marker);
  //           }
  //         } catch (error) {
  //           console.error(`Failed to geocode address for ${user.fullName}:`, error);
  //         }
  //       }
  //     }
  //
  //     return markers;
  //   } catch (error) {
  //     console.error('Error loading user markers:', error);
  //     throw error;
  //   }
  // }
  //
  // // Helper methods to extract information from different user types
  // private getUserAddress(user: UserDTO | SpecialistDTO): string | null {
  //   if ('address' in user) {
  //     // SpecialistDTO has direct address property
  //     return user.address || null;
  //   } else if (user.contactInfo?.address) {
  //     // UserDTO has address in contactInfo
  //     return user.contactInfo.address;
  //   }
  //   return null;
  // }
  //
  // private getUserPhone(user: UserDTO | SpecialistDTO): string | null {
  //   if ('phoneNumber' in user) {
  //     // SpecialistDTO has direct phoneNumber property
  //     return user.phoneNumber || null;
  //   } else if (user.contactInfo?.phoneNumber) {
  //     // UserDTO has phoneNumber in contactInfo
  //     return user.contactInfo.phoneNumber;
  //   }
  //   return null;
  // }
  //
  // private getUserDescription(user: UserDTO | SpecialistDTO): string {
  //   if ('description' in user) {
  //     // SpecialistDTO has direct description property
  //     return user.description || '';
  //   } else if (user.specialist?.description) {
  //     // UserDTO might have specialist profile with description
  //     return user.specialist.description;
  //   } else if (user.role === UserRoleEnum.Client) {
  //     return 'Client looking for professional services';
  //   }
  //   return '';
  // }
  //
  // private getUserCategories(user: UserDTO): string[] | undefined {
  //   if ('categories' in user && user.categories) {
  //     // SpecialistDTO has direct categories property
  //     return user.specialist?.categories?.map(cat => cat.name);
  //   } else if (user.specialist?.categories) {
  //     // UserDTO might have specialist profile with categories
  //     return user.specialist.categories.map(cat => cat.name);
  //   } else if (user.role === UserRoleEnum.Client) {
  //     return ['Service Seeker'];
  //   }
  //   return [];
  // }

  // Search methods for future API endpoints
  // searchNearbyUsers(request: NearbyUsersRequestDto): Observable<(UserDTO | SpecialistDTO)[]> {
  //   // This would be implemented when the backend has a nearby search endpoint
  //   // For now, return all users and filter client-side
  //   const config = this.getMapConfig();
  //   if (!config) {
  //     return of([]);
  //   }
  //
  //   if (config.showSpecialists) {
  //     return this.getSpecialists(1, 100).pipe(
  //       map(response => response.data || [])
  //     );
  //   } else {
  //     return this.getAllUsers(1, 100).pipe(
  //       map(response => {
  //         const allUsers = response.data || [];
  //         return allUsers.filter(user => user.role === UserRoleEnum.Client);
  //       })
  //     );
  //   }
  // }
  //
  // // Update user location (for future implementation)
  // updateUserLocation(userId: string, address: string): Observable<boolean> {
  //   // This would update the user's address in the backend
  //   // Implementation depends on your backend API structure
  //   console.log(`Updating location for user ${userId} to ${address}`);
  //   return of(true);
  // }
}
