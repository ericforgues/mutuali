<template>
  <div class="w-100">
    <portal v-if="!alertCreated" :to="$consts.enums.PORTAL_HEADER">
      <nav-close :to="closeRoute"></nav-close>
    </portal>
    <template v-if="!alertCreated">
      <div class="section section--md section--padding-x section--border-bottom my-4">
        <h1 class="my-4">{{ $t("page-title.add-alert") }}</h1>
      </div>
      <alert-form @submitForm="createAlert" :disabledBtn="isSubmitted" :btnLabel="$t('btn.save-alert')" />
    </template>

    <form-complete
      v-else
      :title="$t('form-complete.create-alert.title')"
      :description="$t('form-complete.create-alert.description')"
      :image="require('@/assets/icons/checklist.png')"
      :ctas="formCompleteCtas"
    />
  </div>
</template>

<script>
import NavClose from "@/components/nav/close";
import FormComplete from "@/components/generic/form-complete";
import AlertForm from "@/components/alert/form";

import { URL_LIST_AD, URL_AD_ALERT_LIST } from "@/consts/urls";

import { createAlert } from "@/services/alert";

export default {
  components: {
    AlertForm,
    FormComplete,
    NavClose
  },
  computed: {
    isConnected() {
      return this.user && this.user.isConnected;
    },
    closeRoute() {
      return { name: this.isConnected ? this.$consts.urls.URL_AD_ALERT_LIST : this.$consts.urls.URL_LIST_AD };
    }
  },
  data() {
    var ctas = [];
    if (this.isConnected) {
      ctas = [
        { action: () => this.$router.push({ name: URL_AD_ALERT_LIST }), text: this.$t("btn.manage-my-alerts") },
        { action: () => this.$router.push({ name: URL_LIST_AD }), text: this.$t("btn.return-map") }
      ];
    } else {
      ctas = [{ action: () => this.$router.push({ name: URL_LIST_AD }), text: this.$t("btn.return-map") }];
    }
    return {
      alertCreated: false,
      isSubmitted: false,
      formCompleteCtas: ctas
    };
  },
  methods: {
    createAlert: async function(input) {
      this.isSubmitted = true;
      let result = await createAlert(input);

      if (result) {
        this.alertCreated = true;
        window.scrollTo(0, 0);
      }
      this.isSubmitted = false;
    }
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
