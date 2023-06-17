import React, { Component } from "react";
import { useLocation } from "react-router";
import { NodeInfo } from "./NodeInfo";
import * as d3 from "d3";

export const ViewGraph = (props) => {
  const location = useLocation();
  return (
    <ViewGraphInternal
      ids={location.state.ids}
      height="800px"
      width="100%"
      {...props}
    />
  );
};

class ViewGraphInternal extends Component {
  constructor(props) {
    super(props);
    this.state = {
      loading: true,
      currentlySelectedNodeInfo: [],
    };
  }

  renderGraph(graph) {
    const nodes = graph.nodes.map((node) => Object.create(node));
    const links = graph.links.map((link) => Object.create(link));

    const nodeInfo = d3.select(".content .nodeInfo");

    const svg = d3
      .select("svg")
      .attr("width", this.props.width)
      .attr("height", this.props.height)
      .call(
        d3.zoom().on("zoom", function (e) {
          d3.selectAll("svg g").attr("transform", e.transform);
        })
      );

    const simulation = d3
      .forceSimulation()
      .force("charge", d3.forceManyBody().strength(-5))
      .force(
        "center",
        d3.forceCenter(
          350, //TODO - calculate center somehow from svg dimension
          350
        )
      );

    // TODO: Attempt to implement dragging, but doesn't work for some reason
    svg.call(
      d3
        .drag()
        .container(svg)
        .subject((e) => simulation.find(e.x, e.y))
        .on("start", (e) => {
          if (!e.active) {
            simulation.alphaTarget(0.3).restart();
          }

          e.subject.fx = e.subject.x;
          e.subject.fy = e.subject.y;
        })
        .on("drag", (e) => {
          e.subject.fx = e.x;
          e.subject.fy = e.y;
        })
        .on("end", (e) => {
          if (!e.active) {
            simulation.alphaTarget(0);
          }

          e.subject.fx = null;
          e.subject.fy = null;
        })
    );
    //

    const linkElements = svg
      .append("g")
      .selectAll("line")
      .data(links)
      .enter()
      .append("line")
      .attr("stroke-width", 1)
      .attr("stroke", "#E5E5E5");

    const nodeElements = svg
      .append("g")
      .selectAll("circle")
      .data(nodes)
      .enter()
      .append("circle")
      .on("hover", function (d) {
        console.log("HOVER");
      })
      .attr("id", function (d) {
        return d.url;
      })
      .attr("class", ".node")
      .attr("r", 3)
      .attr("fill", function (d) {
        return d.color;
      })
      .on("click", function (e, d, i) {
        nodeInfo.selectAll("ul").remove();
        svg
          .selectAll("circle")
          .filter(function () {
            return d3.select(this).attr("fill") == "red";
          })
          .attr("fill", d.color);

        nodeInfo
          .append("div")
          .append("ul")
          .append("li")
          .text("Title: " + (d.title ? d.title : "???"))
          .append("li")
          .text("Url: " + d.url)
          .append("li")
          .text("Crawl time: " + d.crawlTime)
          .append("li")
          .text(d.inWhichGraphs.map((graphId) => graphId));

        d3.select(this).attr("fill", "red");
      });

    simulation.nodes(nodes).on("tick", () => {
      nodeElements.attr("cx", (node) => node.x).attr("cy", (node) => node.y);

      linkElements
        .attr("x1", (link) => link.source.x)
        .attr("y1", (link) => link.source.y)
        .attr("x2", (link) => link.target.x)
        .attr("y2", (link) => link.target.y);
    });

    simulation.force(
      "link",
      d3
        .forceLink(links)
        .id((link) => link.url)
        .distance(20)
    );
  }

  componentDidMount() {
    this.getGraphAsync().then((graph) => this.renderGraph(graph));

    this.setState({ loading: false });
  }

  async getGraphAsync() {
    const graphsJson = [];

    console.log(this.props.ids);

    for (const id of this.props.ids) {
      console.log(id);
      const response = await fetch(`/Record/${id}/livegraph`);
      console.log(response);

      if (response.ok) {
        const graphJson = await response.json();
        graphJson["websiteRecordId"] = id;
        graphsJson.push(graphJson);
        console.log(graphJson);
      }
    }

    const graph = await this.convertGraphsJsonToD3JsonAsync(graphsJson);
    return graph;
  }

  async convertGraphsJsonToD3JsonAsync(graphsJson) {
    let addNewNodes = true;

    const graph = {
      nodes: [],
      links: [],
    };

    for (const graphJson of graphsJson) {
      const recordForGraph = await (
        await fetch(`/Record/${graphJson.websiteRecordId}`)
      ).json();

      for (const node of graphJson.Graph) {
        if (addNewNodes) {
          const alreadyPresentNode = graph.nodes.find(
            (n) => n.url === node.Url
          );

          if (alreadyPresentNode) {
            if (
              Date.parse(recordForGraph.crawlInfo.lastExecution.started) >
              alreadyPresentNode.started
            ) {
              // overwrite with latest data
              alreadyPresentNode.url = node.Url;
              alreadyPresentNode.title = node.Title;
              alreadyPresentNode.crawlTime = node.CrawlTime;
              alreadyPresentNode.started = Date.parse(
                recordForGraph.crawlInfo.lastExecution.started
              );
              alreadyPresentNode.color = this.stringToColour(
                recordForGraph.label
              );
            }

            alreadyPresentNode.inWhichGraphs.push(recordForGraph.label);
          } else {
            graph.nodes.push({
              url: node.Url,
              title: node.Title,
              crawlTime: node.CrawlTime,
              started: Date.parse(
                recordForGraph.crawlInfo.lastExecution.started
              ),
              inWhichGraphs: [recordForGraph.label],
              color: this.stringToColour(recordForGraph.label),
            });
          }
        }

        for (const neighbourUrl of node.Neighbours) {
          try {
            graph.links.push({
              source: node.Url,
              target: neighbourUrl,
              forWhichGraph: recordForGraph.label,
            });
          } catch (except) {
            continue;
          }
        }
      }
    }

    console.log(graph);
    return graph;
  }

  //https://stackoverflow.com/questions/3426404/create-a-hexadecimal-colour-based-on-a-string-with-javascript
  stringToColour(str) {
    var hash = 0;
    for (var i = 0; i < str.length; i++) {
      hash = str.charCodeAt(i) + ((hash << 5) - hash);
    }
    var colour = "#";
    for (var i = 0; i < 3; i++) {
      var value = (hash >> (i * 8)) & 0xff;
      colour += ("00" + value.toString(16)).substr(-2);
    }
    return colour;
  }

  render() {
    let loading = "";
    if (this.state.loading) {
      loading = (
        <p>
          <em>Loading ...</em>
        </p>
      );
    }

    return (
      <div className="content">
        <div
          className="nodeInfo"
          style={{
            position: "absolute",
            zIndex: 1,
          }}
        />
        {loading}
        <svg
          style={{
            zIndex: 0,
          }}
        ></svg>
      </div>
    );
  }
}
