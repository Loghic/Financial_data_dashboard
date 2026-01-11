import sys
import json
import numpy as np
from sklearn.linear_model import LinearRegression
from datetime import datetime, timedelta


def main():
    try:
        # Read JSON input from stdin
        raw_input = sys.stdin.read()
        data = json.loads(raw_input)

        symbol = data.get("Symbol")
        dates = data.get("Dates", [])
        prices = data.get("ClosePrices", [])

        if not symbol or len(prices) < 10:
            raise ValueError("Invalid or insufficient input data")

        # Convert to numpy arrays
        y = np.array(prices, dtype=float)
        X = np.arange(len(y)).reshape(-1, 1)

        # Train linear regression model
        model = LinearRegression()
        model.fit(X, y)

        # Predict future values
        prediction_horizon = 10
        future_X = np.arange(len(y), len(y) + prediction_horizon).reshape(-1, 1)
        predicted_prices = model.predict(future_X)

        # Generate future dates
        last_date = datetime.fromisoformat(dates[-1])
        future_dates = [
            (last_date + timedelta(days=i + 1)).isoformat()
            for i in range(prediction_horizon)
        ]

        # Build output
        result = {
            "Symbol": symbol,
            "Dates": future_dates,
            "PredictedValues": predicted_prices.tolist()
        }

        # Output JSON to stdout
        print(json.dumps(result))

    except Exception as ex:
        error_result = {
            "error": str(ex)
        }
        print(json.dumps(error_result))
        sys.exit(1)


if __name__ == "__main__":
    main()

