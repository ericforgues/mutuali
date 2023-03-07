<template>
  <div class="no-content my-5 py-sm-4">
    <img class="no-content__img mb-4" alt="" :src="require('@/assets/ambiance/bowl-pile.svg')" />
    <p>{{ $t("profile-detail.no-alerts") }}</p>
    <b-button variant="primary" :to="{ name: $consts.urls.URL_AD_ALERT_ADD }">{{ $t("nav.create-alert") }} </b-button>
  </div>
</template>

<script>
export default {};
</script>
