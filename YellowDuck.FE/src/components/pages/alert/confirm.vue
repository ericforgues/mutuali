<template>
  <div class="alert-list w-100 mt-4 mt-md-5">
    <div class="no-content my-5 py-sm-4">
      <img class="no-content__img mb-4" alt="" :src="require('@/assets/ambiance/flying-star.svg')" />
      <p>{{ $t("notification.alert-confirmed") }}</p>
      <b-button variant="primary" :to="{ name: $consts.urls.URL_LIST_AD }">{{ $t("btn.return-map") }} </b-button>
    </div>
  </div>
</template>

<script>
import { confirmAlert } from "@/services/alert";
export default {
  async mounted() {
    await confirmAlert(this.$route.params.id, this.$route.query.email);
  }
};
</script>
