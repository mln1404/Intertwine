# Intertwine Questions collection

Import `Intertwine.Questions.postman_collection.json` into Postman, then edit the collection variables:

- `email` and `password`: credentials for an existing Intertwine user.
- `questionId`: an active question ID.
- `answerId`: an active answer belonging to that question.
- `categoryId`: an existing category ID for the filtered request.

Run **1. Login** first. Its test script stores the returned JWT in `accessToken`, which the protected question requests inherit automatically.

The collection calculates `localDate` from the machine running Postman before each request. This represents the client's local calendar date expected by the API.

The first **6. Submit Answer** request generates and stores `idempotencyKey`. Sending it again unchanged demonstrates a safe replay. Before submitting a different question, answer, or local date, clear the `idempotencyKey` collection variable so the request generates a new key.

Postman may require **Enable SSL certificate verification** to be turned off for the local ASP.NET Core development certificate if that certificate is not trusted on the machine.

## Simple wallet top-up

Import `Intertwine.TopUp.postman_collection.json`, then set its `accessToken` and `creditPackageId` collection variables. The request sends `POST /api/wallet/top-up` with the selected credit package.
