<template>
  <s-form @submit="submitForm" class="alert-form">
    <div class="section section--md section--padding-x section--border-bottom my-4 pb-5 rm-child-margin">
      <s-form-select
        id="adCategory"
        name="adCategory"
        v-model="form.category"
        :label="$t('label-whichCategory')"
        label-class="h2"
        :options="categoryOptions"
        right
        variant="outline-secondary"
        required
        rules="required"
      />
    </div>

    <div v-if="hasTwoCols" class="section section--md section--padding-x section--border-bottom my-4 pb-5 rm-child-margin">
      <div class="alert-form__row">
        <div class="alert-form__col rm-child-margin">
          <s-form-checkbox-group
            v-if="form.category === CATEGORY_PROFESSIONAL_KITCHEN"
            v-model="form.professionalKitchenEquipment"
            id="professionalKitchenEquipment"
            :label="$t('label.ad-professionalKitchenEquipment')"
            label-class="h2"
            name="professionalKitchenEquipment"
            :options="professionalKitchenEquipmentOptions"
          />
          <template v-if="form.category === CATEGORY_DELIVERY_TRUCK">
            <s-form-select
              v-model="form.deliveryTruckType"
              id="deliveryTruckType"
              :label="$t('label.ad-deliveryTruckType')"
              label-class="h2"
              name="deliveryTruckType"
              :options="deliveryTruckTypesOptionsWithAll"
            />
            <s-form-checkbox
              v-model="form.refrigerated"
              id="refrigerated"
              :label="$t('label.ad-refrigerated')"
              name="refrigerated"
            />
            <s-form-checkbox
              v-model="form.canSharedRoad"
              id="canSharedRoad"
              :label="$t('label.ad-canSharedRoad')"
              name="canSharedRoad"
            />
            <s-form-checkbox
              v-model="form.canHaveDriver"
              id="canHaveDriver"
              :label="$t('label.ad-canHaveDriver')"
              name="canHaveDriver"
            />
          </template>
        </div>
      </div>
    </div>
    <div class="section section--md section--padding-x section--border-bottom my-4 pb-5 rm-child-margin">
      <div class="alert-form__row">
        <div class="alert-form__col">
          <fieldset id="availabilitiesFieldset" aria-labelledby="availabilitiesFieldset__legend">
            <legend id="availabilitiesFieldset__legend" class="h2 my-0">{{ $t("label.availability") }}</legend>
            <b-row>
              <b-col>
                <s-form-checkbox
                  v-model="form.dayAvailability"
                  id="dayAvailability"
                  :label="$t('label.ad-dayAvailability')"
                  name="dayAvailability"
                />
              </b-col>
              <b-col>
                <s-form-checkbox
                  v-model="form.eveningAvailability"
                  id="eveningAvailability"
                  :label="$t('label.ad-eveningAvailability')"
                  name="eveningAvailability"
                />
              </b-col>
            </b-row>
          </fieldset>
        </div>
      </div>
    </div>
    <div class="section section--md section--padding-x section--border-bottom my-4 pb-5 rm-child-margin">
      <div class="alert-form__row">
        <div class="alert-form__col">
          <fieldset id="radiusFieldset" aria-labelledby="radiusFieldset__legend">
            <legend id="radiusFieldset__legend" class="h2 my-0">{{ $t("label.radius") }}</legend>
            <s-form-input
              id="radius"
              type="number"
              v-model="form.radius"
              :label="$t('label.radius-of')"
              :placeholder="$t('placeholder.radius-of')"
              name="radius"
              rules="requiredNumeric"
              required
              append="km"
            />
            <s-form-google-autocomplete
              id="address"
              :label="$t('label.near-of')"
              v-model="form.address"
              name="address"
              rules="required|haveLatLong"
              :placeholder="$t('placeholder.ad-address')"
              required
            />
          </fieldset>
        </div>
      </div>
    </div>
    <div v-if="!isConnected" class="section section--md section--padding-x section--border-bottom my-4 pb-5 rm-child-margin">
      <div class="alert-form__row">
        <div class="alert-form__col">
          <fieldset id="emailFieldset" aria-labelledby="emailFieldset__legend">
            <legend id="emailFieldset__legend" class="h2 my-0">{{ $t("label.send-to") }}</legend>
            <s-form-input
              id="email"
              type="email"
              v-model="form.email"
              :label="$t('label.your-email')"
              :placeholder="$t('placeholder.email')"
              name="email"
              rules="required|email"
              required
            />
          </fieldset>
        </div>
      </div>
    </div>

    <div class="section section--md section--padding-x my-4 pb-5 rm-child-margin">
      <div class="fab-container__fab">
        <div class="section section--md">
          <b-button :disabled="disabledBtn" type="submit" variant="primary" size="lg" block :aria-label="$t('sr.edit')">
            <b-icon icon="pencil" class="mr-2" aria-hidden="true"></b-icon>{{ btnLabel }}
          </b-button>
        </div>
      </div>
    </div>
  </s-form>
</template>

<script>
import {
  CATEGORY_PROFESSIONAL_KITCHEN,
  CATEGORY_DELIVERY_TRUCK,
  CATEGORY_STORAGE_SPACE,
  CATEGORY_OTHER
} from "@/consts/categories";

import SForm from "@/components/form/s-form";
import SFormInput from "@/components/form/s-form-input";
import SFormCheckboxGroup from "@/components/form/s-form-checkbox-group";
import SFormCheckbox from "@/components/form/s-form-checkbox";
import SFormSelect from "@/components/form/s-form-select";
import SFormGoogleAutocomplete from "@/components/form/s-form-google-autocomplete";

import { AdCategory } from "@/mixins/ad-category";

import { ProfessionalKitchenEquipment } from "@/mixins/professional-kitchen-equipment";
import { AdDeliveryTruckType } from "@/mixins/ad-delivery-truck-type";
import { AvailabilityWeekday } from "@/mixins/availability-weekday";
const defaultFormValue = {
  category: null,
  professionalKitchenEquipment: [],
  deliveryTruckType: null,
  dayAvailability: false,
  eveningAvailability: false,
  refrigerated: false,
  canHaveDriver: false,
  canSharedRoad: false,
  radius: null,
  address: null,
  email: null
};
export default {
  mixins: [AdCategory, ProfessionalKitchenEquipment, AvailabilityWeekday, AdDeliveryTruckType],
  components: { SForm, SFormCheckboxGroup, SFormCheckbox, SFormSelect, SFormGoogleAutocomplete, SFormInput },
  props: {
    btnLabel: {
      type: String,
      required: true
    },
    disabledBtn: {
      type: Boolean,
      default: false
    },
    value: {
      type: Object,
      default() {
        return { ...defaultFormValue };
      }
    }
  },
  methods: {
    submitForm: function() {
      let input = {};

      const maybeEditedFields = [
        "category",
        "professionalKitchenEquipment",
        "deliveryTruckType",
        "dayAvailability",
        "eveningAvailability",
        "refrigerated",
        "canSharedRoad",
        "canHaveDriver",
        "radius",
        "address",
        "email"
      ];
      for (let maybeEditedField of maybeEditedFields) {
        if (
          Array.isArray(this.value[maybeEditedField]) &&
          this.value[maybeEditedField].sort().join(",") === this.form[maybeEditedField].sort().join(",")
        ) {
          continue;
        }
        if (this.value[maybeEditedField] === this.form[maybeEditedField]) continue;

        input[maybeEditedField] = this.form[maybeEditedField];
      }
      this.$emit("submitForm", input);
    }
  },
  data() {
    return {
      form: { ...this.value },
      professionalKitchenEquipment: [],
      categoryOptions: [
        { value: null, text: this.$t("select.all-equipment") },
        { value: CATEGORY_PROFESSIONAL_KITCHEN, text: this.$t("select.category-professional-kitchen") },
        { value: CATEGORY_DELIVERY_TRUCK, text: this.$t("select.category-delivery-truck") },
        { value: CATEGORY_STORAGE_SPACE, text: this.$t("select.category-storage-space") },
        { value: CATEGORY_OTHER, text: this.$t("select.category-other") }
      ],
      CATEGORY_PROFESSIONAL_KITCHEN,
      CATEGORY_DELIVERY_TRUCK
    };
  },
  computed: {
    isConnected() {
      return this.user && this.user.isConnected;
    },
    deliveryTruckTypesOptionsWithAll() {
      let options = [{ value: null, text: this.$t("select.all-equipment") }];
      if (Array.isArray(this.deliveryTruckTypesOptions)) {
        options = [...options, ...this.deliveryTruckTypesOptions];
      }
      return options;
    },
    hasTwoCols() {
      return this.form.category === CATEGORY_PROFESSIONAL_KITCHEN || this.form.category === CATEGORY_DELIVERY_TRUCK;
    }
  },
  watch: {
    value() {
      this.form = { ...this.value };
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

<style lang="scss">
.alert-form {
  // Override style legend with h2 class
  .h2 {
    padding-bottom: 0;
    padding-top: 0;
    font-size: 2rem;

    @include media-breakpoint-down(lg) {
      font-size: calc(1.325rem + 0.9vw);
    }
  }
}
</style>
