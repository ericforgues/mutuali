import Apollo from "@/graphql/vue-apollo";
import { CreateAlert, UpdateAlert, DeleteAlert, ConfirmAlert } from "./alert.graphql";
import { addMaybeValue } from "@/helpers/graphql";

import { getAddressFromGooglePlace } from "@/services/google-map";
import AuthentificationService from "./authentification";

export async function createAlert(input) {
  let mutationInput = {
    category: input.category,
    address: await getAddressFromGooglePlace(input.address),
    radius: input.radius,
  };

  addMaybeValue(input, mutationInput, "professionalKitchenEquipment");
  addMaybeValue(input, mutationInput, "deliveryTruckType");
  addMaybeValue(input, mutationInput, "dayAvailability");
  addMaybeValue(input, mutationInput, "eveningAvailability");
  addMaybeValue(input, mutationInput, "refrigerated");
  addMaybeValue(input, mutationInput, "canSharedRoad");
  addMaybeValue(input, mutationInput, "canHaveDriver");
  addMaybeValue(input, mutationInput, "email");

  let result = await Apollo.instance.defaultClient.mutate({
    mutation: CreateAlert,
    variables: {
      input: mutationInput
    }
  });

  // Renew the token since we want the new alert in our
  await AuthentificationService.renewToken();
  return result;
}

export async function updateAlert(input) {
  let mutationInput = {
    alertId: input.alertId
  };

  addMaybeValue(input, mutationInput, "category");
  await addMaybeValue(input, mutationInput, "address", getAddressFromGooglePlace);
  addMaybeValue(input, mutationInput, "radius");
  addMaybeValue(input, mutationInput, "professionalKitchenEquipment");
  addMaybeValue(input, mutationInput, "deliveryTruckType");
  addMaybeValue(input, mutationInput, "dayAvailability");
  addMaybeValue(input, mutationInput, "eveningAvailability");
  addMaybeValue(input, mutationInput, "refrigerated");
  addMaybeValue(input, mutationInput, "canSharedRoad");
  addMaybeValue(input, mutationInput, "canHaveDriver");
  addMaybeValue(input, mutationInput, "email");

  if (Object.keys(mutationInput).length > 1) {
    let result = await Apollo.instance.defaultClient.mutate({
      mutation: UpdateAlert,
      variables: {
        input: mutationInput
      }
    });

    return result;
  }
}


export async function deleteAlert(alertId, email) {
  let mutationInput = {
    alertId,
    email
  };

  let result = await Apollo.instance.defaultClient.mutate({
    mutation: DeleteAlert,
    variables: {
      input: mutationInput
    }
  });

  return result;
}

export async function confirmAlert(alertId, email) {
  let mutationInput = {
    alertId,
    email
  };

  let result = await Apollo.instance.defaultClient.mutate({
    mutation: ConfirmAlert,
    variables: {
      input: mutationInput
    }
  });

  return result;
}