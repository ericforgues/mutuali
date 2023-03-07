<template>
  <div class="alert-snippet focus-overflow w-100">
    <div class="section section--full-width-mobile section--sm">
      <UiSnippet
        :snippetIsLink="false"
        :hideActions="false"
        :title="getCategoryLabel(alert.category)"
        :noWrapTitle="false"
        :smallTitle="false"
        :mainRoute="{ name: $consts.urls.URL_AD_ALERT_EDIT, params: { id: alert.id } }"
        :mainRouteLabel="$t('btn.alert-edit')"
      >
        <template #description>
          <p class="text-muted mb-0 mt-1">
            {{ description }}
          </p>
          <p class="text-muted mb-0 mt-1">
            {{ $t("text.alert-radius", { radius: alert.radius }) }}
          </p>
        </template>

        <template #actions>
          <b-button @click="deleteAlert" variant="outline-primary" size="sm" class="mt-2 ml-2">
            <b-icon icon="trash-fill" class="mr-1" aria-hidden="true"></b-icon>
            {{ $t("btn.alert-delete") }}
          </b-button>
        </template>
      </UiSnippet>
    </div>
  </div>
</template>

<script>
import { deleteAlert } from "@/services/alert";
import UiSnippet from "@/components/ui/snippet";
import { AdCategory } from "@/mixins/ad-category";
import { ProfessionalKitchenEquipment } from "@/mixins/professional-kitchen-equipment";
import { AdDeliveryTruckType } from "@/mixins/ad-delivery-truck-type";
import { CATEGORY_PROFESSIONAL_KITCHEN, CATEGORY_DELIVERY_TRUCK } from "@/consts/categories";
import NotificationService from "@/services/notification";

export default {
  mixins: [AdCategory, AdDeliveryTruckType, ProfessionalKitchenEquipment],
  components: {
    UiSnippet
  },
  props: {
    alert: {
      type: Object,
      required: true
    },
    email: {
      type: String
    }
  },
  computed: {
    description() {
      const content = [];
      if (this.alert.dayAvailability) content.push(this.$t("label.ad-dayAvailability"));
      if (this.alert.eveningAvailability) content.push(this.$t("label.ad-eveningAvailability"));
      switch (this.alert.category) {
        case CATEGORY_PROFESSIONAL_KITCHEN:
          for (const professionalKitchenEquipment of this.alert.professionalKitchenEquipment) {
            content.push(this.getProfessionalKitchenEquipmentLabel(professionalKitchenEquipment));
          }
          break;
        case CATEGORY_DELIVERY_TRUCK:
          if (this.alert.deliveryTruckType) content.push(this.getDeliveryTruckTypeLabel(this.alert.deliveryTruckType));
          if (this.alert.canHaveDriver) content.push(this.$t("label.ad-canHaveDriver"));
          if (this.alert.canSharedRoad) content.push(this.$t("label.ad-canSharedRoad"));
          if (this.alert.refrigerated) content.push(this.$t("label.ad-refrigerated"));
          break;
      }
      return content.join(", ");
    }
  },
  methods: {
    async deleteAlert() {
      await deleteAlert(this.alert.id, this.email);
      NotificationService.showSuccess(this.$t("notification.alert-deleted"));
      this.$emit("deleted");
    }
  }
};
</script>

<style lang="scss">
.alert-snippet {
  & {
    background-color: $body-bg;
    border-bottom: 1px solid $gray-200;
  }

  &:first-child {
    border-top: 1px solid $gray-200;
  }
}
</style>
