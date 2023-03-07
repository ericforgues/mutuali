<template>
  <component
    :is="snippetIsLink ? 'RouterLink' : 'div'"
    class="ui-snippet"
    :class="{ 'ui-snippet--link': snippetIsLink }"
    :to="mainRoute"
  >
    <div v-if="imageSrc" class="ui-snippet__img-container">
      <img class="ui-snippet__img" :src="`${imageSrc}?mode=crop&width=200&height=200`" :alt="imageAlt ? imageAlt : ''" />
    </div>

    <div class="ui-snippet__content">
      <slot name="suptitle">
        <p v-if="suptitle" class="my-0 text-uppercase font-weight-bold letter-spacing-wide smaller">{{ suptitle }}</p>
      </slot>

      <slot name="title">
        <component
          :is="titleTag"
          class="ui-snippet__title h6"
          :class="[{ 'ui-snippet__title--ellipse': noWrapTitle }, smallTitle ? 'h6 ui-snippet__title--small' : 'h2']"
        >
          {{ title }}
        </component>
      </slot>

      <slot name="description"></slot>

      <div v-if="!hideActions">
        <b-button v-if="!snippetIsLink" variant="primary" size="sm" class="mt-2" :to="mainRoute">
          {{ mainRouteLabel }}
        </b-button>
        <span v-else class="btn mt-2 btn-primary btn-sm">{{ mainRouteLabel }}</span>
        <slot v-if="!snippetIsLink" name="actions"></slot>
      </div>
    </div>
  </component>
</template>

<script>
export default {
  props: {
    title: {
      type: String,
      required: true
    },
    titleTag: {
      type: String,
      default: "h2"
    },
    suptitle: {
      type: String,
      default: ""
    },
    imageSrc: {
      type: String,
      default: ""
    },
    imageAlt: {
      type: String,
      default: ""
    },
    mainRoute: {
      type: Object,
      required: true
    },
    mainRouteLabel: {
      type: String,
      required: true
    },
    snippetIsLink: Boolean,
    smallTitle: Boolean,
    noWrapTitle: Boolean,
    hideActions: Boolean
  }
};
</script>

<style lang="scss">
.ui-snippet {
  $img-width: #{"clamp(80px, 10vw, 120px)"};

  & {
    align-items: center;
    display: flex;
    min-height: $img-width;
  }

  &--link {
    .ui-snippet__title {
      transition: color 0.1s ease-in-out, text-decoration 0.2s ease-in-out;
      text-decoration: underline;
      text-underline-offset: 2px;
      text-decoration-thickness: 2px;
      text-decoration-color: transparent;
    }

    &:hover {
      text-decoration: none;
      color: inherit;

      .ui-snippet__title {
        text-decoration-color: currentColor;
      }
    }
  }

  &__img-container {
    position: relative;
    align-self: stretch;
    width: $img-width;
    flex: 0 0 auto;
  }

  &__img {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    display: block;
    height: 100%;
    width: 100%;
    object-fit: cover;
    object-position: center;
  }

  &__content {
    display: block;
    flex: 1 1 auto;
    padding: $spacer/2 $spacer;
    overflow: hidden;
  }

  &__title {
    font-family: $font-family-base;
    font-size: $h5-font-size;
    font-weight: 500;
    line-height: 1.2;
    margin: 0;

    &--small {
      font-size: $h6-font-size;
    }

    &--ellipse {
      line-height: inherit;
      white-space: nowrap;
      text-overflow: ellipsis;
      overflow: hidden;
    }
  }
}
</style>
