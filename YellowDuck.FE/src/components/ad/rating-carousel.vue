<template>
  <div v-if="ratings.length > 0" class="mb-5">
    <rate :averageRating="averageRating" :ratingsCount="ratings.length" />
    <carousel v-if="ratings.length > 1">
      <b-carousel-slide v-for="(rating, key) in ratings" :key="key">
        <template #img>
          <rating-card :rating="rating" carousel />
        </template>
      </b-carousel-slide>
    </carousel>
    <rating-card v-else :rating="ratings[0]" carousel />
  </div>
</template>

<script>
import Carousel from "@/components/generic/carousel";
import Rate from "@/components/rating/rate";
import RatingCard from "@/components/rating/card.vue";
import { RatingsCriterias } from "@/mixins/ratings-criterias";

export default {
  mixins: [RatingsCriterias],
  components: {
    Carousel,
    RatingCard,
    Rate
  },
  props: {
    id: {
      type: String,
      required: true
    }
  },
  computed: {
    averageRating: function () {
      return this.ad.averageRating;
    },
    ratings: function () {
      const ratings = this.ad ? this.ad.adRatings : [];
      return this.getRatingsWithCriterias(ratings, ["compliance", "cleanliness", "security"]);
    }
  },
  apollo: {
    ad: {
      query() {
        return this.$options.query.AdById;
      },
      variables() {
        return {
          id: this.id
        };
      }
    }
  }
};
</script>

<graphql>
query AdById($id: ID!) {
  ad(id: $id) {
    id
    averageRating
    adRatings {
      id
      cleanlinessRating
      securityRating
      complianceRating
      createdAt
      raterUser {
        id
        profile {
          id
          ... on UserProfileGraphType {
            publicName
          }
        }
      }
    }
  }
}
</graphql>