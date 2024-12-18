import { Component, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Member } from '../../_models/member';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TimeagoModule } from 'ngx-timeago';
import { DatePipe } from '@angular/common';
import { MemberMessagesComponent } from "../member-messages/member-messages.component";
import { MessageService } from '../../_services/message.service';
import { PresenseService } from '../../_services/presense.service';
import { AccountService } from '../../_services/account.service';
import { HubConnectionState } from '@microsoft/signalr';

@Component({
  selector: 'app-member-detail', // The selector used to include this component in templates.
  standalone: true, // Indicates this component is standalone and doesn't belong to an NgModule.
  imports: [TabsModule, GalleryModule, TimeagoModule, DatePipe, MemberMessagesComponent], // Modules and components imported for use in this component.
  templateUrl: './member-detail.component.html', // Path to the HTML template.
  styleUrl: './member-detail.component.css' // Path to the CSS file for styling.
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  
  // Injecting services using Angular's DI.
  presenseService = inject(PresenseService);
  private route = inject(ActivatedRoute); // To access route data and parameters.
  private router = inject(Router); // For navigation and query parameter handling.
  private messageService = inject(MessageService); // Service for handling messages.
  private accountService = inject(AccountService); // Service for managing user accounts.

  @ViewChild('memberTabs', { static: true }) memberTabs?: TabsetComponent; 
  // Reference to the TabsetComponent instance in the template.

  member: Member = {} as Member; // Holds the details of the selected member.
  images: GalleryItem[] = []; // Array to store image items for the gallery.
  activeTab?: TabDirective; // Stores the currently active tab.

  ngOnDestroy(): void {
    // Stop the SignalR hub connection when the component is destroyed.
    this.messageService.stopHubConnection();
  }

  ngOnInit(): void {
    // Subscribe to route data to get the member details.
    this.route.data.subscribe({
      next: (data) => {
        this.member = data['member']; // Assign the member data from the resolver.
        // Map the member's photos into gallery items.
        this.member && this.member.photos.map(p => this.images.push(new ImageItem({ src: p.url, thumb: p.url })));
      }
    });

    // Subscribe to changes in the route parameters.
    this.route.paramMap.subscribe({
      next: _ => this.onRouteParamsChange() // Trigger actions when route parameters change.
    });

    // Subscribe to query parameter changes.
    this.route.queryParams.subscribe({
      next: (params) => {
        // If the 'tab' query parameter exists, activate the corresponding tab.
        params['tab'] && this.selectTab(params['tab']);
      }
    });
  }

  selectTab(heading: string) {
    // Activates a tab based on its heading.
    if (this.memberTabs) {
      const messageTabs = this.memberTabs?.tabs.find(x => x.heading === heading);
      if (messageTabs) messageTabs.active = true; // Set the found tab as active.
    }
  }

  onRouteParamsChange() {
    // Handles changes in route parameters.
    const user = this.accountService.currentUser(); // Get the current user.
    if (!user) return; // Do nothing if the user is not logged in.

    // If the hub is connected and the active tab is "Messages", recreate the hub connection.
    if (this.messageService.hubConnection?.state === HubConnectionState.Connected && this.activeTab?.heading === 'Messages') {
      this.messageService.hubConnection?.stop().then(() => {
        this.messageService.createHubConnection(user, this.member.username); // Recreate connection for the current member.
      });
    }
  }

  onTabActivated(data: TabDirective) {
    // Handles actions when a tab is activated.
    this.activeTab = data; // Set the currently active tab.

    // Update the URL query parameters to reflect the active tab.
    this.router.navigate([], {
      relativeTo: this.route, 
      queryParams: { tab: this.activeTab.heading }, // Add the active tab to query parameters.
      queryParamsHandling: 'merge' // Merge with existing query parameters.
    });

    // If the "Messages" tab is activated, start the SignalR hub connection for the member.
    if (this.activeTab.heading === "Messages" && this.member) {
      const user = this.accountService.currentUser(); // Get the current user.
      if (!user) return; // Do nothing if the user is not logged in.

      this.messageService.createHubConnection(user, this.member.username); // Start the hub connection.
    } else {
      // Stop the SignalR hub connection if switching away from the "Messages" tab.
      this.messageService.stopHubConnection();
    }
  }
}
