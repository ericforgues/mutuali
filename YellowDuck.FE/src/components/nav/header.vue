<template>
  <nav-base>
    <b-navbar-nav class="nav-header ml-auto mr-n2">
      <b-nav-item link-classes="text-dark" :to="{ name: $consts.urls.URL_LIST_AD }">{{ $t("btn.list-ad") }}</b-nav-item>
      <template v-if="isConnected">
        <template v-if="isAdmin">
          <b-nav-item link-classes="position-relative text-dark" :to="{ name: $consts.urls.URL_LIST_USERS }">
            {{ $t("btn.manage-users") }}
          </b-nav-item>
        </template>
        <template v-else>
          <b-nav-item link-classes="position-relative text-dark" :to="{ name: $consts.urls.URL_LIST_CONVERSATION }">
            {{ $t("btn.list-conversation") }}
            <span v-if="unreadMessageCount > 0" class="nav-header__notification"></span>
          </b-nav-item>
        </template>
        <b-nav-item-dropdown right no-caret>
          <template #button-content>
            <span class="text-dark">
              <span class="nav-header__profile-icon" aria-hidden="true"><b-icon-person-circle></b-icon-person-circle></span>
              {{ $t("nav.profile") }}
            </span>
          </template>
          <template v-if="!isAdmin">
            <b-dropdown-item :to="{ name: $consts.urls.URL_USER_PROFILE_DETAIL, params: { id: userProfileId } }">{{
              $t("btn.my-profile")
            }}</b-dropdown-item>
            <b-dropdown-item :to="{ name: $consts.urls.URL_MANAGE_ADS }">{{ $t("btn.manage-my-ads") }}</b-dropdown-item>
            <b-dropdown-item :to="{ name: $consts.urls.URL_AD_ALERT_LIST }">{{ $t("btn.manage-my-alerts") }}</b-dropdown-item>
          </template>
          <b-dropdown-item :to="{ name: $consts.urls.URL_PROFILE_EDIT }">{{ $t("btn.edit-my-profile") }}</b-dropdown-item>
          <b-dropdown-item :to="{ name: $consts.urls.URL_ACCOUNT_SETTINGS }">{{ $t("btn.account-settings") }}</b-dropdown-item>
          <b-dropdown-item @click="logout">{{ $t("btn.logout") }}</b-dropdown-item>
        </b-nav-item-dropdown>
      </template>
      <template v-else>
        <b-nav-item link-classes="text-dark" :to="{ name: $consts.urls.URL_LOGIN }">{{ $t("btn.login") }}</b-nav-item>
      </template>
      <b-nav-item link-classes="text-dark" @click="changeLang">
        <span class="sr-only">{{ $t("btn.change-lang") }}</span>
        <span aria-hidden>{{ $t("btn.change-lang-shortcut") }}</span>
      </b-nav-item>
    </b-navbar-nav>
  </nav-base>
</template>

<graphql>
query LocalUser {
  user @client {
    isConnected
  }
}

query Me {
  me {
    id
    profile {
      id
    }
  }
}
</graphql>

<script>
import { TWILIO_EVENT_MESSAGE_ADDED, TWILIO_EVENT_MESSAGE_READ } from "@/consts/twilio";
import { USER_TYPE_ADMIN } from "@/consts/enums";

import NavBase from "@/components/nav/base";

import AuthentificationService from "@/services/authentification";
import TwilioService from "@/services/twilio";

import i18nHelpers from "@/helpers/i18n";
import EventBus from "@/helpers/event-bus";
import debounce from "@/helpers/debounce";

export default {
  data() {
    return {
      unreadMessageCount: 0
    };
  },
  beforeDestroy() {
    EventBus.$off(TWILIO_EVENT_MESSAGE_READ, this.onMessageRead);
    EventBus.$off(TWILIO_EVENT_MESSAGE_ADDED, this.onMessageAdded);
  },
  components: { NavBase },
  computed: {
    isConnected: function() {
      return this.user && this.user.isConnected;
    },
    isAdmin: function() {
      return this.user && AuthentificationService.getUserType() === USER_TYPE_ADMIN;
    },
    userProfileId: function() {
      if (this.me) {
        return this.me.profile.id;
      }
      return "";
    }
  },
  methods: {
    onMessageAdded: function(event) {
      // Since the getUnreadMessagesCount is not 100% real time, we wait 3 seconds before displayed the badge
      // https://tinyurl.com/m6s482d3
      let vm = this;
      let debounceFn = debounce(() => {
        vm.updateUnreadMessagesCount();
      }, 750);
      debounceFn();
    },
    onMessageRead: function(event) {
      // Since the getUnreadMessagesCount is not 100% real time, we wait 3 seconds before displayed the badge
      // https://tinyurl.com/m6s482d3
      let vm = this;
      let debounceFn = debounce(() => {
        vm.updateUnreadMessagesCount();
      }, 750);
      debounceFn();
    },
    async updateUnreadMessagesCount() {
      this.unreadMessageCount = await TwilioService.getUnreadMessagesCount();
    },
    changeLang: function() {
      i18nHelpers.changeLang();
    },
    logout: function() {
      AuthentificationService.logout();
    }
  },
  apollo: {
    me: {
      query() {
        return this.$options.query.Me;
      },
      skip() {
        return !this.isConnected;
      }
    },
    user: {
      query() {
        return this.$options.query.LocalUser;
      },
      result({ data }) {
        if (data && data.user.isConnected) {
          if (!this.isAdmin) {
            //clear EventBus before readded since we can pass more then one time here
            EventBus.$off(TWILIO_EVENT_MESSAGE_READ, this.onMessageRead);
            EventBus.$off(TWILIO_EVENT_MESSAGE_ADDED, this.onMessageAdded);

            EventBus.$on(TWILIO_EVENT_MESSAGE_READ, this.onMessageRead);
            EventBus.$on(TWILIO_EVENT_MESSAGE_ADDED, this.onMessageAdded);

            this.updateUnreadMessagesCount();
          }
        }
      }
    }
  }
};
</script>

<style lang="scss">
.nav-header {
  .nav-link {
    position: relative;

    &.nav-link,
    &.router-link-active {
      color: $green-darker;

      &::before {
        background-color: transparent;
        content: "";
        display: block;
        height: 2px;
        pointer-events: none;
        position: absolute;
        bottom: #{$spacer / -2};
        left: #{$spacer / 2};
        right: #{$spacer / 2};
        transition: background-color 0.2s ease-in-out;
      }
    }

    &.nav-link:not(.router-link-active) {
      &:hover {
        &::before {
          background-color: $green-darker;
        }
      }
    }

    &.router-link-active {
      &::before {
        background-color: $yellow;
      }
    }
  }

  &__profile-icon {
    display: none;
    @include media-breakpoint-up(sm) {
      display: inline;
    }
  }

  &__notification {
    background: $danger;
    border-radius: $spacer;
    display: block;
    height: $spacer / 2;
    width: $spacer / 2;
    position: absolute;
    top: $spacer / 2;
    right: 0;
  }
}
</style>
