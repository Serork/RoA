using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;

namespace RoA.Core;

// calamity fables
public class VerletPoint {
    public Vector2 position, oldPosition;
    public bool locked;
    public int tileCollideStyle;
    public bool CollidingWithTiles => tileCollideStyle > 0;
    public bool PassThroughPlatforms => tileCollideStyle == 1;
    public Vector2? customGravity;
    public Func<Vector2, Vector2, Vector2> customCollide = null;

    public VerletPoint(Vector2 _position, bool _locked = false) {
        position = _position;
        oldPosition = _position;
        locked = _locked;
        customGravity = null;
    }

    public static implicit operator Vector2(VerletPoint point) => point.position;

    public static List<VerletPoint> SimpleSimulation(List<VerletPoint> segments, float segmentDistance, int loops = 10, float gravity = 0.3f, float momentumConservation = 1f) => SimpleSimulation(segments, segmentDistance, loops, gravity * Vector2.UnitY, momentumConservation);

    public static List<VerletPoint> SimpleSimulation(List<VerletPoint> segments, float segmentDistance, int loops, Vector2 gravity, float momentumConservation = 1f) {
        //https://youtu.be/PGk0rnyTa1U?t=400 verlet integration chains reference here
        foreach (VerletPoint segment in segments) {
            if (!segment.locked) {
                Vector2 positionBeforeUpdate = segment.position;

                segment.position += (segment.position - segment.oldPosition) * momentumConservation; // This adds conservation of energy to the segments. This makes it super bouncy and shouldnt be used but it's really funny

                if (segment.customGravity.HasValue)
                    segment.position += segment.customGravity.Value;
                else
                    segment.position += gravity; //=> This adds gravity to the segments. 

                segment.oldPosition = positionBeforeUpdate;
            }
        }

        int segmentCount = segments.Count;

        for (int k = 0; k < loops; k++) {
            for (int j = 0; j < segmentCount - 1; j++) {
                VerletPoint pointA = segments[j];
                VerletPoint pointB = segments[j + 1];
                Vector2 segmentCenter = (pointA.position + pointB.position) / 2f;
                Vector2 segmentDirection = Utils.SafeNormalize(pointA.position - pointB.position, Vector2.UnitY);

                if (!pointA.locked)
                    pointA.position = segmentCenter + segmentDirection * segmentDistance / 2f;

                if (!pointB.locked)
                    pointB.position = segmentCenter - segmentDirection * segmentDistance / 2f;

                segments[j] = pointA;
                segments[j + 1] = pointB;
            }
        }

        return segments;
    }


    public static void ComplexSimulation(List<VerletPoint> points, List<VerletStick> segments, int loops = 10, float gravity = 0.3f, float momentumConservation = 1f) => ComplexSimulation(points, segments, loops, Vector2.UnitY * gravity, momentumConservation);

    public static void ComplexSimulation(List<VerletPoint> points, List<VerletStick> segments, int loops, Vector2 gravity, float momentumConservation = 1f) {
        //https://youtu.be/PGk0rnyTa1U?t=400 verlet integration chains reference here
        foreach (VerletPoint point in points) {
            if (!point.locked) {
                Vector2 positionBeforeUpdate = point.position;
                Vector2 velocity = (point.position - point.oldPosition) * momentumConservation;

                if (point.customGravity.HasValue)
                    velocity += point.customGravity.Value;
                else
                    velocity += gravity; //=> This adds gravity to the segments. 

                if (point.customCollide != null)
                    velocity = point.customCollide(point.position, velocity);

                if (point.CollidingWithTiles)
                    velocity = TileCollision(point.position, velocity, point.PassThroughPlatforms);

                point.position += velocity;
                point.oldPosition = positionBeforeUpdate;
            }
        }

        for (int k = 0; k < loops; k++) {
            foreach (VerletStick stick in segments) {
                Vector2 segmentCenter = (stick.point1.position + stick.point2.position) / 2f;
                Vector2 segmentDirection = Utils.SafeNormalize(stick.point1.position - stick.point2.position, Vector2.Zero);

                if (!stick.point1.locked) {
                    if (stick.point1.customCollide != null)
                        stick.point1.position += stick.point1.customCollide(stick.point1.position, segmentCenter + segmentDirection * stick.lenght / 2f - stick.point1.position);
                    else if (stick.point1.CollidingWithTiles)
                        stick.point1.position += TileCollision(stick.point1.position, segmentCenter + segmentDirection * stick.lenght / 2f - stick.point1.position, stick.point1.PassThroughPlatforms);
                    else
                        stick.point1.position = segmentCenter + segmentDirection * stick.lenght / 2f;
                }

                if (!stick.point2.locked) {
                    if (stick.point2.customCollide != null)
                        stick.point2.position += stick.point2.customCollide(stick.point2.position, segmentCenter + segmentDirection * stick.lenght / 2f - stick.point2.position);
                    else if (stick.point2.CollidingWithTiles)
                        stick.point2.position += TileCollision(stick.point2.position, segmentCenter - segmentDirection * stick.lenght / 2f - stick.point2.position, stick.point2.PassThroughPlatforms);
                    else
                        stick.point2.position = segmentCenter - segmentDirection * stick.lenght / 2f;

                }
            }
        }
    }

    public static Vector2 TileCollision(Vector2 position, Vector2 velocity, bool fallThrough) {
        Vector2 newVel = Collision.noSlopeCollision(position - new Vector2(3, 3), velocity, 6, 6, fallThrough, true);

        if (Math.Abs(newVel.X) < Math.Abs(velocity.X))
            velocity.X *= 0;

        if (Math.Abs(newVel.Y) < Math.Abs(velocity.Y))
            velocity.Y *= 0;

        return velocity;
    }

    public override string ToString() => position.ToString() + (locked ? " (locked)" : " (free)");
}

public class VerletStick {
    public VerletPoint point1;
    public VerletPoint point2;
    public float lenght;

    public VerletStick(VerletPoint point1, VerletPoint point2) {
        this.point1 = point1;
        this.point2 = point2;
        lenght = (point1.position - point2.position).Length();
    }

    public override string ToString() => point1.ToString() + " - " + point2.ToString() + " (" + lenght.ToString() + ")";
}

public class VerletNet {
    public List<VerletStick> segments;
    public List<VerletPoint> points;

    /// <summary>
    /// The extremities of every net segment. Useful if you need to do operations on the end of all extremities
    /// </summary>
    public List<VerletPoint> extremities;
    /// <summary>
    /// The individual chains making up the net
    /// </summary>
    public List<List<VerletPoint>> chains;
    public List<float> chainLenghts;

    public void Simulate(int iterations, float gravity) => VerletPoint.ComplexSimulation(points, segments, iterations, gravity);
    public void Simulate(int iterations, Vector2 gravity) => VerletPoint.ComplexSimulation(points, segments, iterations, gravity);
    public void Simulate(int iterations, float gravity, float momentumConservation = 1f) => VerletPoint.ComplexSimulation(points, segments, iterations, gravity, momentumConservation);
    public void Simulate(int iterations, Vector2 gravity, float momentumConservation = 1f) => VerletPoint.ComplexSimulation(points, segments, iterations, gravity, momentumConservation);


    public void Update(int iterations, float gravity, float momentumConservation = 1f) => Update(iterations, gravity * Vector2.UnitY, momentumConservation);
    public void Update(int iterations, Vector2 gravity, float momentumConservation = 1f) {
        Simulate(iterations, gravity, momentumConservation);
    }

    public void AddChain(VerletPoint point1, VerletPoint point2, int segmentCount, float droop = 0f, float primResolution = 1f) {
        //Add the extremities to the list of points
        if (!points.Contains(point1))
            points.Add(point1);
        if (!points.Contains(point2))
            points.Add(point2);
        //Again
        if (!extremities.Contains(point1))
            extremities.Add(point1);
        if (!extremities.Contains(point2))
            extremities.Add(point2);

        List<VerletPoint> newChain = new();
        chains.Add(newChain);
        newChain.Add(point1);

        float newChainLenght = 0;

        for (int i = 1; i < segmentCount; i++) {
            Vector2 position = Vector2.Lerp(point1.position, point2.position, i / (float)segmentCount);
            if (droop != 0f)
                position.Y += MathUtils.SineBumpEasing(i / (float)segmentCount) * droop;

            VerletPoint point = new VerletPoint(position);

            //Connect the first point to the rest of the chain
            if (i == 1)
                segments.Add(new VerletStick(point1, point));
            //Connect the chain together
            else
                segments.Add(new VerletStick(points[^1], point));

            //Add the point
            points.Add(point);
            newChain.Add(point);

            newChainLenght += segments[^1].lenght;
        }

        //Connect the last point with the end extremity to finish it off
        segments.Add(new VerletStick(points[^1], point2));
        newChain.Add(point2);
        newChainLenght += segments[^1].lenght;

        //Save the lenght of the chain
        chainLenghts.Add(newChainLenght);
    }

    public void AddChain(VerletPoint point1, VerletPoint point2, int segmentCount) {
        //Add the extremities to the list of points
        if (!points.Contains(point1))
            points.Add(point1);
        //Again
        if (!extremities.Contains(point1))
            extremities.Add(point1);
        if (!extremities.Contains(point2))
            extremities.Add(point2);

        List<VerletPoint> newChain = new();
        chains.Add(newChain);
        newChain.Add(point1);

        float newChainLenght = 0;

        for (int i = 1; i < segmentCount; i++) {
            Vector2 position = Vector2.Lerp(point1.position, point2.position, i / (float)segmentCount);
            VerletPoint point = new VerletPoint(position);

            //Connect the first point to the rest of the chain
            if (i == 1)
                segments.Add(new VerletStick(point1, point));
            //Connect the chain together
            else
                segments.Add(new VerletStick(points[^1], point));

            //Add the point
            points.Add(point);
            newChain.Add(point);

            newChainLenght += segments[^1].lenght;
        }

        //Connect the last point with the end extremity to finish it off
        segments.Add(new VerletStick(points[^1], point2));
        newChain.Add(point2);
        if (!points.Contains(point2))
            points.Add(point2);
        newChainLenght += segments[^1].lenght;

        //Save the lenght of the chain
        chainLenghts.Add(newChainLenght);
    }


    public void AddChain(params VerletPoint[] chainPoints) {
        //Add the extremities to the list of points
        if (!points.Contains(chainPoints[0]))
            points.Add(chainPoints[0]);
        //Again
        if (!extremities.Contains(chainPoints[0]))
            extremities.Add(chainPoints[0]);
        if (!extremities.Contains(chainPoints[^1]))
            extremities.Add(chainPoints[^1]);
        List<VerletPoint> newChain = new();
        chains.Add(newChain);
        newChain.Add(chainPoints[0]);

        float newChainLenght = 0;

        for (int i = 1; i < chainPoints.Length; i++) {
            //Connect the first point to the rest of the chain
            if (i == 1)
                segments.Add(new VerletStick(chainPoints[0], chainPoints[i]));
            //Connect the chain together
            else
                segments.Add(new VerletStick(points[^1], chainPoints[i]));

            //Add the point
            points.Add(chainPoints[i]);
            newChain.Add(chainPoints[i]);

            newChainLenght += segments[^1].lenght;
        }

        //Save the lenght of the chain
        chainLenghts.Add(newChainLenght);
    }

    public void AddSimpleStick(VerletPoint point1, VerletPoint point2) {
        //Add the extremities to the list of points
        if (!points.Contains(point1))
            points.Add(point1);
        if (!points.Contains(point2))
            points.Add(point2);

        List<VerletPoint> newChain = new();
        newChain.Add(point1);
        newChain.Add(point2);

        chainLenghts.Add(point1.position.Distance(point2.position));
        chains.Add(newChain);
    }

    public void RemoveFirstChain(bool ignoreShared = true) {
        List<VerletPoint> firstChain = chains[0];

        //Remove the stored chain lenght
        chainLenghts.RemoveAt(0);

        VerletPoint start = firstChain[0];
        VerletPoint end = firstChain[firstChain.Count - 1];

        if (ignoreShared) {
            for (int i = 1; i < chains.Count; i++)
                firstChain.RemoveAll(p => chains[i].Contains(p));

            if (firstChain.Count == 0)
                return;

            //If the extremities were shared, don't delete them
            if (start != firstChain[0])
                start = null;
            if (end != firstChain[firstChain.Count - 1])
                end = null;
        }

        //Remove the points
        points.RemoveAll(p => firstChain.Contains(p));
        //Remove the extremities
        extremities.RemoveAll(p => p == start || p == end);
        //Remove the sticks
        segments.RemoveAll(s => firstChain.Contains(s.point1) || firstChain.Contains(s.point2));

        //Finally, clear and remove the chains
        firstChain.Clear();
        chains[0].Clear();
        chains.RemoveAt(0);
    }

    public void RemoveLastChain(bool ignoreShared = true) {
        List<VerletPoint> lastChain = chains[^1];

        //Remove the stored chain lenght
        chainLenghts.RemoveAt(chainLenghts.Count - 1);

        VerletPoint start = lastChain[0];
        VerletPoint end = lastChain[lastChain.Count - 1];

        if (ignoreShared) {
            for (int i = 0; i < chains.Count - 1; i++)
                lastChain.RemoveAll(p => chains[i].Contains(p));

            if (lastChain.Count == 0)
                return;

            //If the extremities were shared, don't delete them
            if (start != lastChain[0])
                start = null;
            if (end != lastChain[lastChain.Count - 1])
                end = null;
        }

        //Remove the points
        points.RemoveAll(p => lastChain.Contains(p));
        //Remove the extremities
        extremities.RemoveAll(p => p == start || p == end);
        //Remove the sticks
        segments.RemoveAll(s => lastChain.Contains(s.point1) || lastChain.Contains(s.point2));

        //Finally, clear and remove the chains
        lastChain.Clear();
        chains[^1].Clear();
        chains.RemoveAt(chains.Count - 1);
    }

    public VerletNet() {
        segments = new();
        points = new();
        extremities = new();
        chains = new();
        chainLenghts = new();
    }

    public void Clear() {
        segments.Clear();
        points.Clear();

        extremities.Clear();

        chains.Clear();
        chainLenghts.Clear();
    }
}
