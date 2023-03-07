<template>
  <div v-if="userProfile" class="alert-list w-100 mt-4 mt-md-5">
    <div class="section section--sm mb-4">
      <h1>{{ $t("title.manage-my-alerts") }}</h1>
    </div>
    <template v-if="alerts.length > 0">
      <div class="mb-5 pt-3">
        <alert-snippet
          v-for="alert in alerts"
          :key="alert.id"
          :alert="alert"
          :email="me.email"
          @deleted="refreshAlerts()"
          sectionWidth="sm"
          smallTitle
        />
      </div>
    </template>
    <alert-no-content v-if="alerts.length === 0" class="my-5 py-sm-5" />
    <div v-else class="alert-list__bottom section section--md">
      <div class="mx-4">
        <b-button variant="primary" size="lg" block :to="{ name: $consts.urls.URL_AD_ALERT_ADD }">{{
          $t("nav.create-alert")
        }}</b-button>
      </div>
    </div>
  </div>
</template>

<script>
import AlertNoContent from "@/components/alert/no-content.vue";
import AlertSnippet from "@/components/alert/snippet.vue";

export default {
  components: {
    AlertNoContent,
    AlertSnippet
  },
  computed: {
    profileId: function() {
      return this.me.profile.id;
    },
    alerts: function() {
      return this.userProfile.user ? this.userProfile.user.alerts : [];
    }
  },
  methods: {
    refreshAlerts() {
      this.$apollo.queries.userProfile.refetch({
        id: this.profileId
      });
    }
  },
  apollo: {
    me: {
      query() {
        return this.$options.query.Me;
      }
    },
    userProfile: {
      query() {
        return this.$options.query.UserProfileById;
      },
      variables() {
        return {
          id: this.profileId
        };
      },
      skip() {
        return !this.me;
      }
    }
  }
};
</script>
<graphql>
query Me {
  me {
    id
    email
    profile {
      id
    }
  }
}
query UserProfileById($id: ID!) {
  userProfile(id: $id) {
    id
    user {
      alerts {
        id
        category
        radius
        dayAvailability
        eveningAvailability
        professionalKitchenEquipment
        deliveryTruckType
        refrigerated
        canSharedRoad
        canHaveDriver
      }
    }
  }
}
</graphql>

<style lang="scss">
.alert-list {
  position: relative;
  width: 100%;
  z-index: 0;

  &__bottom {
    position: absolute;
    bottom: $spacer;
    z-index: 1;
  }
}
</style>
