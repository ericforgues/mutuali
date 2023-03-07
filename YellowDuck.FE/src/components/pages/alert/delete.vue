<template>
  <div class="alert-list w-100 mt-4 mt-md-5">
    <div class="no-content my-5 py-sm-4">
      <img class="no-content__img mb-4" alt="" :src="require('@/assets/ambiance/nothing.svg')" />
      <p>{{ $t("notification.alert-deleted") }}</p>
      <b-button v-if="isConnected" variant="primary" :to="{ name: $consts.urls.URL_AD_ALERT_LIST }"
        >{{ $t("btn.manage-my-alerts") }}
      </b-button>
      <b-button v-else variant="primary" :to="{ name: $consts.urls.URL_LIST_AD }">{{ $t("btn.return-map") }} </b-button>
    </div>
  </div>
</template>

<script>
import { deleteAlert } from "@/services/alert";
export default {
  computed: {
    isConnected() {
      return this.user && this.user.isConnected;
    }
  },
  async mounted() {
    await deleteAlert(this.$route.params.id, this.$route.query.email);
  },
  apollo: {
    user: {
      query() {
        return this.$options.query.LocalUser;
      }
    }
  }
};
</script>
<graphql>
query LocalUser {
  user @client {
    isConnected
  }
}
</graphql>
